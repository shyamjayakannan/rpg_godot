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
		AddItemPickupsToScene(GlobalLevelManager.Instance.GetDroppedItems(GetTree().CurrentScene.SceneFilePath));
	}

	private void AddItemPickupsToScene(List<(Items, Vector2)> items)
	{
		if (items == null || items.Count == 0)
			return;

		foreach (Node child in GetChildren())
		{
			if (child is not LevelTileMap)
				continue;

			foreach ((Items, Vector2) tuple in items)
			{
				ItemPickup itemPickup = (ItemPickup)itemPickupScene.Instantiate();
				itemPickup.Item = tuple.Item1;
				itemPickup.GlobalPosition = tuple.Item2;
				itemPickup.IsDroppedItem = true;
				itemPickup.SavedPosition = tuple.Item2;
				child.AddChild(itemPickup);
			}

			return;
		}
	}
}
