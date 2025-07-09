using Godot;

public partial class LevelTileMap : TileMapLayer
{
	// methods
	public override void _Ready()
	{
		GlobalPlayerManager.Instance.SetParent(this);
		GlobalLevelManager.Instance.ChangeTileMapBounds(GetTileMapBounds());
	}

	private Vector2[] GetTileMapBounds()
	{
		// create maps with one extra cell on each side so that autotile will allow the path to end with straight tile
		return new[] {
			(GetUsedRect().Position + Vector2.One) * TileSet.TileSize + GlobalPosition,
			(GetUsedRect().End - Vector2.One) * TileSet.TileSize + GlobalPosition,
		};
	}
}
