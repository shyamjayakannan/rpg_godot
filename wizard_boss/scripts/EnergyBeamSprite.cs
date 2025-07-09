using Godot;

public partial class EnergyBeamSprite : Sprite2D
{
    // Exports
    [Export]
    private float speed = 100;

    // methods
    public override void _Process(double delta)
    {
        RegionRect = new Rect2(
            RegionRect.Position + new Vector2(speed * (float)delta, 0),
            RegionRect.Size
        );
    }
}
