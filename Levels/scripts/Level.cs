using System.Collections.Generic;
using Godot;

public partial class Level : Node2D
{
	// Exports
	[Export]
	private AudioStream music;

	// private
	private PackedScene itemPickupScene = GD.Load<PackedScene>("res://Items/itemPickup/ItemPickup.tscn");

	// methods
	public override void _Ready()
	{
		GlobalAudioManager.Instance.PlayAudio(music);
		GlobalPlayerManager.Instance.SetParent(this);
		GlobalLevelManager.Instance.ChangeTileMapBounds(GetTileMapBounds());
		AddItemPickupsToScene(GlobalLevelManager.Instance.GetDroppedItems(GetTree().CurrentScene.SceneFilePath));
	}

	private Vector2[] GetTileMapBounds()
	{
		foreach (Node child in GetChildren())
		{
			if (child is TileMapLayer tileMapLayer)
			{
				return new Vector2[2] {
					tileMapLayer.GetUsedRect().Position * tileMapLayer.TileSet.TileSize + GlobalPosition,
					tileMapLayer.GetUsedRect().End * tileMapLayer.TileSet.TileSize + GlobalPosition,
				};
			}
		}

		return System.Array.Empty<Vector2>();
	}

	private void AddItemPickupsToScene(List<(Items, Vector2, int)> items)
	{
		if (items == null || items.Count == 0)
			return;

		foreach ((Items, Vector2, int) tuple in items)
		{
			ItemPickup itemPickup = (ItemPickup)itemPickupScene.Instantiate();
			YSortHandler.AddToScene(itemPickup, GlobalPlayerManager.Instance.Player);
			itemPickup.Item = tuple.Item1;
			itemPickup.GlobalPosition = tuple.Item2;
			itemPickup.IsDroppedItem = true;
			itemPickup.SavedPosition = tuple.Item2;
		}
	}
}
