using Godot;

public class EnergyBeamSprite : Sprite
{
    // Exports
    [Export]
    private float speed = 100;

    // methods
    public override void _Process(float delta)
    {
        RegionRect = new Rect2(
            RegionRect.Position + new Vector2(speed * delta, 0),
            RegionRect.Size
        );
    }
}
