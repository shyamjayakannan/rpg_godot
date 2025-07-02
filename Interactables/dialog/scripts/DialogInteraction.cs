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
    public bool RunEditorFunction
    {
        get => false;
        set
        {
            if (value)
            {
                // MyEditorFunction();
                // Reset to false so you can trigger again
                PropertyListChangedNotify();
            }
        }
    }
    [Export]
    private readonly List<DialogItemResource> dialogItemResources = new List<DialogItemResource>();

    // private
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

        foreach (DialogItemResource dialogItemResource in dialogItemResources)
            SetChildResources(dialogItemResource);

        area2D.Connect("area_entered", this, nameof(OnArea2DAreaEntered));
        area2D.Connect("area_exited", this, nameof(OnArea2DAreaExited));
    }

    public override string _GetConfigurationWarning()
    {
        return dialogItemResources.Count > 0 ? "" : "please add at least one DialogItemResource";
    }

    public override void OnInteractPressed()
    {
        EmitSignal(nameof(PlayerInteracted));

        async void Wait()
        {
            // need to wait for two idle frames so that animation plays and npc faces player
            await ToSignal(GetTree(), "idle_frame");
            await ToSignal(GetTree(), "idle_frame");

            DialogSystem.Instance.ShowDialog(dialogItemResources);

            if (!DialogSystem.Instance.IsConnected(nameof(DialogSystem.Finished), this, nameof(OnFinished)))
                DialogSystem.Instance.Connect(nameof(DialogSystem.Finished), this, nameof(OnFinished));
        }

        Wait();
    }

    protected override void OnArea2DAreaEntered(Area2D area)
    {
        enabled = dialogItemResources.FindIndex(dialogItemResource => dialogItemResource.QuestConditionResource.CheckIsActivated()) >= 0;

        if (!enabled || dialogItemResources.Count == 0)
            return;

        base.OnArea2DAreaEntered(area);
        animationPlayer.Play("show");
    }

    protected override void OnArea2DAreaExited(Area2D area)
    {
        if (!enabled || dialogItemResources.Count == 0)
            return;

        base.OnArea2DAreaExited(area);
        animationPlayer.Play("hide");
    }

    private void OnFinished()
    {
        EmitSignal(nameof(Finished));
    }

    private void SetChildResources(DialogItemResource dialogItemResource)
    {
        if (dialogItemResource is DialogTextResource)
            return;

        IEnumerable<DialogItemResource> children = null;
        QuestConditionResource parentQuestCondition = dialogItemResource.QuestConditionResource;
        NpcResource parentNpc = dialogItemResource.NpcResource;

        if (dialogItemResource is DialogBranchResource branch)
            children = branch.DialogItemResources;
        else if (dialogItemResource is DialogChoiceResource choice)
            children = choice.DialogBranchResources;

        if (children == null)
            return;

        foreach (DialogItemResource child in children)
        {
            if (child.QuestConditionResource == null)
                child.QuestConditionResource = parentQuestCondition;

            if (child.NpcResource == null)
                child.NpcResource = parentNpc;

            SetChildResources(child);
        }
    }
}
// using System.Collections.Generic;
// using Godot;
// using MonoCustomResourceRegistry;

// [Tool]
// [RegisteredType(nameof(DialogInteraction), "res://GUI/dialogSystem/icons/chat_bubbles.png", nameof(Node2D))]
// public class DialogInteraction : Interactables
// {
//     // Signals
//     [Signal]
//     public delegate void PlayerInteracted();
//     [Signal]
//     public delegate void Finished();

//     // Exports
//     [Export]
//     private readonly bool enabled = true;

//     // private
//     private AnimationPlayer animationPlayer;
//     private readonly List<DialogItem> dialogItems = new List<DialogItem>();
//     private Area2D area2D;

//     // methods
//     public override void _Ready()
//     {
//         animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
//         area2D = GetNode<Area2D>("Area2D");

//         if (Engine.EditorHint)
//             return;

//         area2D.Connect("area_entered", this, nameof(OnArea2DAreaEntered));
//         area2D.Connect("area_exited", this, nameof(OnArea2DAreaExited));

//         foreach (Node child in GetChildren())
//             if (child is DialogItem dialogItem)
//                 dialogItems.Add(dialogItem);
//     }

//     public override string _GetConfigurationWarning()
//     {
//         bool atLeastOneValidChild = false;

//         foreach (Node child in GetChildren())
//             if (child is DialogItem)
//                 atLeastOneValidChild = true;

//         return atLeastOneValidChild ? "" : "please add at least one DialogItem as child";
//     }

//     public override void OnInteractPressed()
//     {
//         EmitSignal(nameof(PlayerInteracted));

//         async void Wait()
//         {
//             // need to wait for two idle frames so that animation plays and npc faces player
//             await ToSignal(GetTree(), "idle_frame");
//             await ToSignal(GetTree(), "idle_frame");

//             DialogSystem.Instance.ShowDialog(dialogItems);

//             if (!DialogSystem.Instance.IsConnected(nameof(DialogSystem.Finished), this, nameof(OnFinished)))
//                 DialogSystem.Instance.Connect(nameof(DialogSystem.Finished), this, nameof(OnFinished));
//         }

//         Wait();
//     }

//     protected override void OnArea2DAreaEntered(Area2D area)
//     {
//         if (!enabled || dialogItems.Count == 0)
//             return;

//         base.OnArea2DAreaEntered(area);
//         animationPlayer.Play("show");
//     }

//     protected override void OnArea2DAreaExited(Area2D area)
//     {
//         if (!enabled || dialogItems.Count == 0)
//             return;

//         base.OnArea2DAreaExited(area);
//         animationPlayer.Play("hide");
//     }

//     private void OnFinished()
//     {
//         EmitSignal(nameof(Finished));
//     }
// }
