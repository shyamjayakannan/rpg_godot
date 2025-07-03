using Godot;
using MonoCustomResourceRegistry;

#if TOOLS
public abstract partial class DialogItem : Node2D
{
    // private
    private EditorSelection editorSelection;

    // methods
    private void OnSelectionChanged()
    {
        if (editorSelection == null)
            return;

        Godot.Collections.Array selectedNodes = editorSelection.GetSelectedNodes();

        // not this because in case of duplication, the duplicate also references the same examplesystem.
        // because of this, when on duplication the duplicate receives a child examplesystem, the examplesystem is not
        // quefreed because the parent's examplesystem variable references the original's examplesystem which is
        // already quefreed.
        // if (IsInstanceValid(ExampleSystem))
        // {
        //     GD.Print(this);
        //     ExampleSystem.QueueFree();
        //     ExampleSystem = null;
        // }

        foreach (Node child in GetChildren())
        {
            if (child is DialogSystem)
            {
                child.QueueFree();

                // need to set the referencing variable to null (not done automatically on quefree)
                ExampleSystem = null;
            }
        }

        if (selectedNodes.Count == 0 || this != selectedNodes[0])
            return;

        ExampleSystem = (DialogSystem)dialogSystemScene.Instance();
        ExampleSystem.Offset = GetParentGlobalPosition() + new Vector2(32, -200);
        AddChild(ExampleSystem);

        CheckNpcResource();
        CheckQuestConditionResource();
        SetEditorDisplay();
    }
}
#endif

[Tool]
[RegisteredType(nameof(DialogItem), "res://GUI/dialogSystem/icons/chat_bubble.png", nameof(Node2D))]
public abstract partial class DialogItem : Node2D
{
    //Exports
    [Export]
    public DialogItemResource DialogItemResource
    {
        get => dialogItemResource;
        set
        {
            dialogItemResource = (DialogItemResource)value?.Duplicate();

            if (dialogItemResource != null)
                OnResourceLoaded();
        }
    }

    // private
    private DialogItemResource dialogItemResource;
    private readonly PackedScene dialogSystemScene = GD.Load<PackedScene>("res://GUI/dialogSystem/DialogSystem.tscn");

    // properties
    protected DialogSystem ExampleSystem { get; private set; }

    // methods
    // remember to call base._Ready() in the _Ready of all inheriting classes
    public override void _Ready()
    {
#if TOOLS
        if (Engine.EditorHint)
        {
            editorSelection = new EditorPlugin().GetEditorInterface().GetSelection();
            editorSelection.Connect("selection_changed", this, nameof(OnSelectionChanged));
            return;
        }
#endif

        CheckNpcResource();
        CheckQuestConditionResource();
    }

    private void CheckNpcResource()
    {
        if (DialogItemResource.NpcResource != null)
            return;

        for (Node p = GetParent(); p != null; p = p.GetParent())
        {
            if (p is DialogItem dialogItem)
            {
                if (dialogItem.DialogItemResource.NpcResource != null)
                    DialogItemResource.NpcResource = dialogItem.DialogItemResource.NpcResource;

                return;
            }

            if (p is Npc npc && npc.NpcResource != null)
            {
                DialogItemResource.NpcResource = npc.NpcResource;
                return;
            }
        }
    }

    private void CheckQuestConditionResource()
    {
        if (DialogItemResource.QuestConditionResource != null)
            return;

        for (Node p = GetParent(); p != null; p = p.GetParent())
            if (p is DialogItem dialogItem && dialogItem.DialogItemResource.QuestConditionResource != null)
                DialogItemResource.QuestConditionResource = dialogItem.DialogItemResource.QuestConditionResource;
    }

    private Vector2 GetParentGlobalPosition()
    {
        for (Node p = GetParent(); p != null; p = p.GetParent())
        {
            if (p is Node2D node2D)
                return node2D.GlobalPosition;
        }

        return Vector2.Zero;
    }

    public virtual void SetEditorDisplay()
    {
        ExampleSystem?.CommonDisplay(DialogItemResource.NpcResource);
    }

    public virtual void OnResourceLoaded()
    {
        if (!DialogItemResource.IsConnected("changed", this, nameof(SetEditorDisplay)))
            DialogItemResource.Connect("changed", this, nameof(SetEditorDisplay));
    }
}
