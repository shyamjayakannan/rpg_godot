using System.Collections.Generic;
using Godot;
using MonoCustomResourceRegistry;

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
    private string firstDialogPath;

    // private
    private DialogInteractionResource dialogInteractionResource;
    private PackedScene dialogTextScene = GD.Load<PackedScene>("res://Interactables/dialog/DialogText.tscn");
    private PackedScene dialogChoiceScene = GD.Load<PackedScene>("res://Interactables/dialog/DialogChoice.tscn");
    private PackedScene dialogBranchScene = GD.Load<PackedScene>("res://Interactables/dialog/DialogBranch.tscn");
    private AnimationPlayer animationPlayer;
    private Area2D area2D;
    private bool enabled;

    // methods
    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        area2D = GetNode<Area2D>("Area2D");

        if (Engine.EditorHint)
            return;

        if (DialogInteractionResource == null || dialogInteractionResource.DialogItemResources.Count == 0)
            DialogInteractionResource = GD.Load<DialogInteractionResource>(firstDialogPath);

        area2D.Connect("area_entered", this, nameof(OnArea2DAreaEntered));
        area2D.Connect("area_exited", this, nameof(OnArea2DAreaExited));
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

        if (firstDialogPath != null && firstDialogPath != "")
            return "";

        return "please add at least one DialogItem/DialogItemResource or set first dialog path";
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
        List<DialogItemResource> items = new List<DialogItemResource>(((DialogChoiceResource)dialogItem.DialogItemResource).DialogBranchResources);

        if (dialogItem is DialogBranch)
            items = new List<DialogItemResource>(((DialogBranchResource)dialogItem.DialogItemResource).DialogItemResources);

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

    public void ChangeDialog(string path)
    {
        DialogInteractionResource = GD.Load<DialogInteractionResource>(path);
    }

}
