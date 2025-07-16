using Godot;

namespace Rpg
{
    public partial class YSortAnchor : Area2D
    {
        // Exports
        [Export]
        public float DefaultDifference { get; private set; }

        // methods
        public override void _Ready()
        {
            CallDeferred(MethodName.SetDifference, DefaultDifference);
        }

        private void SetDifference(float difference)
        {
            ((YSortHandler)GetParent().GetParent()).SetDifference(difference);
        }
    }
}
