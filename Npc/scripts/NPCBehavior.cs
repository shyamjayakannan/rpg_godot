using Godot;

namespace Rpg
{
    [GlobalClass, Icon("res://Npc/icons/npc_behavior.png")]
    public abstract partial class NPCBehavior : Node2D
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
}
