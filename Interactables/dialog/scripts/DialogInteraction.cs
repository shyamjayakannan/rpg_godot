using System;
using System.Collections.Generic;
using Godot;
using MonoCustomResourceRegistry;
using Newtonsoft.Json;

[Tool]
[RegisteredType(nameof(DialogInteraction), "res://GUI/dialogSystem/icons/chat_bubbles.png", nameof(Node2D))]
public class DialogInteraction : Interactables
{
    // Signals
    [Signal]
    public delegate void PlayerInteracted();
    [Signal]
    public delegate void Finished();

    // Exports
    [Export]
    private DialogInteractionResource DialogInteractionResource
    {
        get => dialogInteractionResource;
        set => dialogInteractionResource = (DialogInteractionResource)value?.Duplicate();
    }
    [Export]
    private bool SetDialogItems
    {
        get => false;
        set
        {
            if (!value)
                return;

            SetDialogChildren();
            PropertyListChangedNotify();
        }
    }
    [Export]
    private bool GetDialogItems
    {
        get => false;
        set
        {
            if (!value)
                return;

            GetDialogChildren();
            PropertyListChangedNotify();
        }
    }
    [Export]
    private string npcDialogFolder;

    // private
    private DialogInteractionResource dialogInteractionResource;
    private PackedScene dialogTextScene = GD.Load<PackedScene>("res://Interactables/dialog/DialogText.tscn");
    private PackedScene dialogChoiceScene = GD.Load<PackedScene>("res://Interactables/dialog/DialogChoice.tscn");
    private PackedScene dialogBranchScene = GD.Load<PackedScene>("res://Interactables/dialog/DialogBranch.tscn");
    private AnimationPlayer animationPlayer;
    private Area2D area2D;
    private bool enabled;
    private Dictionary<string, string> questTitleToFile = new Dictionary<string, string>();

    // methods
    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        area2D = GetNode<Area2D>("Area2D");

        if (Engine.EditorHint)
            return;

        OnGameLoaded();

        area2D.Connect("area_entered", this, nameof(OnArea2DAreaEntered));
        area2D.Connect("area_exited", this, nameof(OnArea2DAreaExited));
        GlobalQuestManager.Instance.Connect(nameof(GlobalQuestManager.QuestUpdated), this, nameof(ChangeDialog));
        GlobalSaveManager.Instance.Connect(nameof(GlobalSaveManager.GameLoaded), this, nameof(OnGameLoaded));
    }

    private void OnGameLoaded()
    {
        LoadQuestMapping();
        string fileName = GlobalQuestManager.Instance.FindQuestForNpc(questTitleToFile);

        if (fileName == "not found")
            DialogInteractionResource = GD.Load<DialogInteractionResource>($"{npcDialogFolder}0.tres");
        else
            DialogInteractionResource = GD.Load<DialogInteractionResource>($"{npcDialogFolder}{fileName}");
    }

    private void LoadQuestMapping()
    {
        File file = new File();
        file.Open($"{npcDialogFolder}questMap.json", File.ModeFlags.Read);
        string json = file.GetAsText();
        file.Close();
        questTitleToFile = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
    }

    public void ChangeDialog(string title, bool isStarted)
    {
        if (!isStarted)
            return;

        if (questTitleToFile.TryGetValue(title, out string fileName))
            DialogInteractionResource = GD.Load<DialogInteractionResource>($"{npcDialogFolder}{fileName}");
    }

    public override string _GetConfigurationWarning()
    {
        int atLeastOneValidChild = 0;

        foreach (Node child in GetChildren())
            if (child is DialogItem)
                atLeastOneValidChild++;

        if (atLeastOneValidChild > 0)
            return "";

        if (DialogInteractionResource != null && DialogInteractionResource.DialogItemResources.Count > 0)
            return "";

        if (npcDialogFolder != null && npcDialogFolder != "")
            return "";

        return "please add at least one DialogItem/DialogItemResource or set npc dialog folder path";
    }

    public override void OnInteractPressed()
    {
        EmitSignal(nameof(PlayerInteracted));

        async void Wait()
        {
            // need to wait for two idle frames so that animation plays and npc faces player
            await ToSignal(GetTree(), "idle_frame");
            await ToSignal(GetTree(), "idle_frame");

            DialogSystem.Instance.ShowDialog(DialogInteractionResource.DialogItemResources, this);

            if (!DialogSystem.Instance.IsConnected(nameof(DialogSystem.Finished), this, nameof(OnFinished)))
                DialogSystem.Instance.Connect(nameof(DialogSystem.Finished), this, nameof(OnFinished));
        }

        Wait();
    }

    protected override void OnArea2DAreaEntered(Area2D area)
    {
        enabled = DialogInteractionResource.DialogItemResources.FindIndex(dialogItemResource => dialogItemResource.QuestConditionResource == null || dialogItemResource.QuestConditionResource.CheckIsActivated()) >= 0;

        if (!enabled || DialogInteractionResource.DialogItemResources.Count == 0)
            return;

        base.OnArea2DAreaEntered(area);
        animationPlayer.Play("show");
    }

    protected override void OnArea2DAreaExited(Area2D area)
    {
        if (!enabled || DialogInteractionResource.DialogItemResources.Count == 0)
            return;

        base.OnArea2DAreaExited(area);
        animationPlayer.Play("hide");
    }

    private void OnFinished()
    {
        EmitSignal(nameof(Finished));
    }

    private void SetDialogChildren()
    {
        foreach (Node child in GetChildren())
            if (child is DialogItem)
                child.QueueFree();

        foreach (DialogItemResource dialogItemResource in DialogInteractionResource.DialogItemResources)
            AddDialogItem(dialogItemResource, this);
    }

    private void GetDialogChildren()
    {
        DialogInteractionResource.DialogItemResources.Clear();

        foreach (Node child in GetChildren())
            if (child is DialogItem dialogItem)
                DialogInteractionResource.DialogItemResources.Add(GetDialogs(dialogItem));
    }

    private void AddDialogItem(DialogItemResource dialogItemResource, Node node)
    {
        DialogItem dialogItem = (DialogItem)dialogTextScene.Instance();

        if (dialogItemResource is DialogChoiceResource)
            dialogItem = (DialogItem)dialogChoiceScene.Instance();
        else if (dialogItemResource is DialogBranchResource)
            dialogItem = (DialogItem)dialogBranchScene.Instance();

        node.AddChild(dialogItem);
        dialogItem.DialogItemResource = dialogItemResource;
        SetDialogs(dialogItem);
        dialogItem.Owner = Owner;
    }

    private void SetDialogs(DialogItem dialogItem)
    {
        if (dialogItem is DialogText)
            return;

        // need to make local copy if the reference changes in each iteration (the resources dont change but the variable referencing them changes in each iteration)
        List<DialogItemResource> items = null;

        if (dialogItem is DialogBranch)
            items = new List<DialogItemResource>(((DialogBranchResource)dialogItem.DialogItemResource).DialogItemResources);
        else if (dialogItem is DialogChoice)
            items = new List<DialogItemResource>(((DialogChoiceResource)dialogItem.DialogItemResource).DialogBranchResources);

        foreach (DialogItemResource dialogItemResource in items)
            AddDialogItem(dialogItemResource, dialogItem);
    }

    private DialogItemResource GetDialogs(DialogItem dialogItem)
    {
        if (dialogItem is DialogText)
            return dialogItem.DialogItemResource;

        if (dialogItem is DialogBranch)
        {
            DialogBranchResource dialogBranchResource = (DialogBranchResource)dialogItem.DialogItemResource;
            dialogBranchResource.DialogItemResources.Clear();

            foreach (Node child in dialogItem.GetChildren())
                if (child is DialogItem item)
                    dialogBranchResource.DialogItemResources.Add(GetDialogs(item));

            return dialogBranchResource;
        }

        if (dialogItem is DialogChoice)
        {
            DialogChoiceResource dialogChoiceResource = (DialogChoiceResource)dialogItem.DialogItemResource;
            dialogChoiceResource.DialogBranchResources.Clear();

            foreach (Node child in dialogItem.GetChildren())
                if (child is DialogBranch item)
                    dialogChoiceResource.DialogBranchResources.Add((DialogBranchResource)GetDialogs(item));

            return dialogChoiceResource;
        }

        return null;
    }
}
