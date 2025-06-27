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

        if (IsInstanceValid(ExampleSystem))
            ExampleSystem.QueueFree();

        if (selectedNodes.Count == 0 || this != selectedNodes[0])
            return;

        ExampleSystem = (DialogSystem)dialogSystemScene.Instance();
        ExampleSystem.Offset = GetParentGlobalPosition() + new Vector2(32, -200);
        AddChild(ExampleSystem);

        CheckNpcResource();
        ExampleSystem.CommonDisplay(NpcResource);
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
    public NpcResource NpcResource
    {
        get => npcResource;
        set
        {
            npcResource = value;

            if (Engine.EditorHint && ExampleSystem != null)
                SetEditorDisplay();
        }
    }

    // private
    public NpcResource npcResource;

    // private
    private PackedScene dialogSystemScene = GD.Load<PackedScene>("res://GUI/dialogSystem/DialogSystem.tscn");

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
    }

    private void CheckNpcResource()
    {
        if (NpcResource != null)
            return;

        for (Node p = GetParent(); p != null; p = p.GetParent())
        {
            if (p is DialogItem dialogItem && dialogItem.NpcResource != null)
            {
                NpcResource = dialogItem.NpcResource;
                return;
            }

            if (p is Npc npc && npc.NpcResource != null)
            {
                NpcResource = npc.NpcResource;
                return;
            }
        }
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

    }
}
