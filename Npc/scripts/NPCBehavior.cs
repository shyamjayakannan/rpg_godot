using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(NPCBehavior), "res://Npc/icons/npc_behavior.png", nameof(Node2D))]
public abstract class NPCBehavior : Node2D
{
    // properties
    protected Npc Npc { get; private set; }

    // methods
    public override void _Ready()
    {
        if (GetParent() is Npc parent)
            Npc = parent;
    }

    protected abstract void Start();
}
