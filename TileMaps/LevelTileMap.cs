using Godot;

public class LevelTileMap : TileMap
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
		return new Vector2[]{
			new Vector2((GetUsedRect().Position + Vector2.One) * CellQuadrantSize) + GlobalPosition,
			new Vector2((GetUsedRect().End - Vector2.One) * CellQuadrantSize) + GlobalPosition,
		};
	}
}
