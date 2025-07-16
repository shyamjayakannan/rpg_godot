using Godot;

// since this area is immediately removed and added as the parent, we need to connect its quefree to its
// earlier parent's quefree
namespace Rpg
{
    public partial class NpcMonitorArea : Area2D
    {
        // private
        private WanderBehavior wanderBehavior;

        // methods
        public override void _Ready()
        {
            if (wanderBehavior != null)
                wanderBehavior.Connect(Node.SignalName.TreeExited, new(this, Node.MethodName.QueueFree));
            else if (GetParent() is WanderBehavior behavior)
                wanderBehavior = behavior;
        }
    }
}
