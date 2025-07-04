using System.Collections.Generic;
using Godot;

public class Level : Node2D
{
	// Exports
	[Export]
	private readonly AudioStream music;

	// private
	private readonly PackedScene itemPickupScene = GD.Load<PackedScene>("res://Items/itemPickup/ItemPickup.tscn");

	// methods
	public override void _Ready()
	{
		GlobalAudioManager.Instance.PlayAudio(music);
		AddItemPickupsToScene(GlobalLevelManager.Instance.GetDroppedItems(GetTree().CurrentScene.Filename));
	}

	private void AddItemPickupsToScene(List<(Items, Vector2)> items)
	{
		if (items == null || items.Count == 0)
			return;

		foreach (Node child in GetChildren())
		{
			if (!(child is LevelTileMap))
				continue;

			foreach ((Items, Vector2) tuple in items)
			{
				ItemPickup itemPickup = (ItemPickup)itemPickupScene.Instance();
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
