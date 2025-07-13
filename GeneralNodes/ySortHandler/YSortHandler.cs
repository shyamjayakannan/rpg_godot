using Godot;

[Tool]
public partial class YSortHandler : Node2D
{
    // Exports
    [Export]
    public float YSortOrigin
    {
        get => ySortOrigin;
        set
        {
            if (child != null)
                SetChild(value - ySortOrigin);

            ySortOrigin = value;
        }
    }

    // private
    private float ySortOrigin;
    private Node2D child;
    private float difference;
    public static PackedScene YSortHandlerScene { get; private set; } = GD.Load<PackedScene>("res://GeneralNodes/ySortHandler/YSortHandler.tscn");

    // methods
    public override void _Ready()
    {
        child = (Node2D)GetChild(0);
        child.Position = Vector2.Zero;
        GlobalPosition = new(GlobalPosition.X, GlobalPosition.Y - ySortOrigin);
        SetChild(YSortOrigin);

        if (Engine.IsEditorHint())
            return;

        CollisionShape2D collisionShape2D = child.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");

        if (collisionShape2D != null)
            child.GetNode<YSortAnchor>("YSortAnchor").AddChild(collisionShape2D.Duplicate());
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
            return;

        GlobalPosition = new(child.GlobalPosition.X, child.GlobalPosition.Y + difference);
        child.Position = new(0, -difference);
    }

    public void SetChild(float _difference)
    {
        GlobalPosition = new(GlobalPosition.X, GlobalPosition.Y + _difference);
        child.Position = new(child.Position.X, child.Position.Y - _difference);
        difference = -child.Position.Y;
    }

    public static void AddToScene(Node2D child, Node sibling)
    {
        YSortHandler ySortHandler = (YSortHandler)YSortHandlerScene.Instantiate();
        ySortHandler.AddChild(child);
        ySortHandler.child = child;
        YSortHandler siblingYSortHandler = (YSortHandler)sibling.GetParent();
        ySortHandler.SetChild(siblingYSortHandler.ySortOrigin);
        siblingYSortHandler.GetParent().AddChild(ySortHandler);
    }
}
