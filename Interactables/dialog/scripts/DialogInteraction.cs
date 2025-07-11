using System;
using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;

[Tool]
[GlobalClass, Icon("res://GUI/dialogSystem/icons/chat_bubbles.png")]
public partial class DialogInteraction : Interactables
{
    // Signals
    [Signal]
    public delegate void PlayerInteractedEventHandler();
    [Signal]
    public delegate void FinishedEventHandler();

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
            NotifyPropertyListChanged();
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
            NotifyPropertyListChanged();
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
    private Dictionary<string, string> questTitleToFile = new();

    // methods
    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        area2D = GetNode<Area2D>("Area2D");

        if (Engine.IsEditorHint())
            return;

        OnGameLoaded();

        area2D.Connect(Area2D.SignalName.AreaEntered, new(this, MethodName.OnArea2DAreaEntered));
        area2D.Connect(Area2D.SignalName.AreaExited, new(this, MethodName.OnArea2DAreaExited));
        GlobalQuestManager.Instance.Connect(GlobalQuestManager.SignalName.QuestUpdated, new(this, MethodName.ChangeDialog));
        GlobalSaveManager.Instance.Connect(GlobalSaveManager.SignalName.GameLoaded, new(this, MethodName.OnGameLoaded));
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
        FileAccess file = FileAccess.Open($"{npcDialogFolder}questMap.json", FileAccess.ModeFlags.Read);
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

    public override string[] _GetConfigurationWarnings()
    {
        int atLeastOneValidChild = 0;

        foreach (Node child in GetChildren())
            if (child is DialogItem)
                atLeastOneValidChild++;

        if (atLeastOneValidChild > 0)
            return Array.Empty<string>();

        if (DialogInteractionResource != null && DialogInteractionResource.DialogItemResources.Count > 0)
            return Array.Empty<string>();

        if (npcDialogFolder != null && npcDialogFolder != "")
            return Array.Empty<string>();

        return new string[1] { "please add at least one DialogItem/DialogItemResource or set npc dialog folder path" };
    }

    public override void OnInteractPressed()
    {
        EmitSignal(SignalName.PlayerInteracted);

        async void Wait()
        {
            // need to wait for two idle frames so that animation plays and npc faces player
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            DialogSystem.Instance.ShowDialog(DialogInteractionResource.DialogItemResources, this);

            if (!DialogSystem.Instance.IsConnected(DialogSystem.SignalName.Finished, new(this, MethodName.OnFinished)))
                DialogSystem.Instance.Connect(DialogSystem.SignalName.Finished, new(this, MethodName.OnFinished));
        }

        Wait();
    }

    protected override void OnArea2DAreaEntered(Area2D area)
    {
        enabled = false;

        for (int i = 0; i < DialogInteractionResource.DialogItemResources.Count; i++)
        {
            DialogItemResource dialogItemResource = DialogInteractionResource.DialogItemResources[i];

            if (dialogItemResource.QuestConditionResource == null || dialogItemResource.QuestConditionResource.CheckIsActivated())
            {
                enabled = true;
                break;
            }
        }

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
        EmitSignal(SignalName.Finished);
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
        DialogItem dialogItem = (DialogItem)dialogTextScene.Instantiate();

        if (dialogItemResource is DialogChoiceResource)
            dialogItem = (DialogItem)dialogChoiceScene.Instantiate();
        else if (dialogItemResource is DialogBranchResource)
            dialogItem = (DialogItem)dialogBranchScene.Instantiate();

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
            items = new(((DialogBranchResource)dialogItem.DialogItemResource).DialogItemResources);
        else if (dialogItem is DialogChoice)
            items = new(((DialogChoiceResource)dialogItem.DialogItemResource).DialogBranchResources);

        foreach (DialogItemResource dialogItemResource in items)
            AddDialogItem(dialogItemResource, dialogItem);
    }

    private static DialogItemResource GetDialogs(DialogItem dialogItem)
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
