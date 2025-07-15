using Godot;

public partial class LevelTileMapLayer : TileMapLayer
{
    // Exports
    [Export]
    private int collisionLayer = 5;

    // methods
    public override void _Ready()
    {
        TileSet = (TileSet)TileSet.Duplicate();
        TileSet.SetPhysicsLayerCollisionLayer(0, (uint)(1 << (collisionLayer - 1)));
    }
}
