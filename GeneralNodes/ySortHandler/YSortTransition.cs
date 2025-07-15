using Godot;

public partial class YSortTransition : Area2D
{
    // Exports
    [Export]
    private Location location;

    // private
    private int ySortOrigin;
    private enum Location
    {
        Up,
        Down
    }

    // methods
    public override void _Ready()
    {
        TileMapLayer parent = (TileMapLayer)GetParent();
        ySortOrigin = parent.YSortOrigin + parent.TileSet.TileSize.Y / 2;
        Connect(Area2D.SignalName.AreaExited, new(this, MethodName.OnAreaExited));
    }

    private void OnAreaExited(Area2D area2D)
    {
        if (area2D is not YSortAnchor)
            return;

        PhysicsBody2D body = (PhysicsBody2D)area2D.GetParent();

        // 16 (collision layer 5) is the base
        body.CollisionMask = (uint)(16 << (ySortOrigin / 64));
        YSortHandler ySortHandler = (YSortHandler)body.GetParent();
        ySortHandler.SetYSortOriginWithoutChild(ySortOrigin);

        switch (location)
        {
            case Location.Up:
                ySortHandler.SetPhysicsProcess(body.GlobalPosition.Y <= GlobalPosition.Y);
                break;

            case Location.Down:
                ySortHandler.SetPhysicsProcess(body.GlobalPosition.Y > GlobalPosition.Y);
                break;
        }
    }
}
