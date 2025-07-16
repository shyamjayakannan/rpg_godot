using Godot;

namespace Rpg
{
    [Tool]
    public partial class YSortHandler : Node2D
    {
        // Exports
        [Export]
        public int YSortOrigin
        {
            get => ySortOrigin;
            set
            {
                if (child != null)
                    SetDifference(value - ySortOrigin);

                ySortOrigin = value;
            }
        }

        // private
        private int ySortOrigin;
        private Node2D child;
        private float difference;
        public static PackedScene YSortHandlerScene { get; private set; } = GD.Load<PackedScene>("res://GeneralNodes/ySortHandler/YSortHandler.tscn");

        // methods
        public override void _Ready()
        {
            child = (Node2D)GetChild(0);
            child.Position = Vector2.Zero;
            GlobalPosition = new(GlobalPosition.X, GlobalPosition.Y - YSortOrigin);
            SetDifference(YSortOrigin);

            if (Engine.IsEditorHint())
                return;

            // 16 (collision layer 5) is the base
            if (child is PhysicsBody2D physicsBody2D)
                physicsBody2D.CollisionMask = (uint)(16 << (YSortOrigin / 64));
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!Engine.IsEditorHint())
                SetChild();
        }

        private void SetChild()
        {
            GlobalPosition = new(child.GlobalPosition.X, child.GlobalPosition.Y + difference);
            child.Position = new(0, -difference);
        }

        public void SetDifference(float _difference)
        {
            difference += _difference;

            if (Engine.IsEditorHint())
                SetChild();
        }

        public void SetYSortOriginWithoutChild(int value)
        {
            difference += value - ySortOrigin;
            ySortOrigin = value;
        }

        public static YSortHandler AddToScene(Node2D child, Node sibling)
        {
            YSortHandler ySortHandler = (YSortHandler)YSortHandlerScene.Instantiate();
            YSortHandler siblingYSortHandler = (YSortHandler)sibling.GetParent();
            ySortHandler.AddChild(child);
            ySortHandler.YSortOrigin = siblingYSortHandler.YSortOrigin;
            siblingYSortHandler.GetParent().AddChild(ySortHandler);
            return ySortHandler;
        }
    }
}
