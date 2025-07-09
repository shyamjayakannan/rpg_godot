using System.Collections.Generic;
using Godot;

public partial class GlobalLevelManager : Node
{
	// Signals
	[Signal]
	public delegate void TileMapBoundsChangedEventHandler(Vector2[] newBounds);
	[Signal]
	public delegate void LevelLoadedEventHandler();
	[Signal]
	public delegate void LevelLoadStartedEventHandler();

	// properties
	public Vector2[] CurrentTileMapBounds { get; private set; }
	public static GlobalLevelManager Instance { get; private set; }
	public string TargetTransitionArea { get; private set; }
	public Vector2 PositionOffset { get; private set; } = Vector2.Zero;
	public Dictionary<string, List<(Items, Vector2)>> DroppedItems { get; set; } = new Dictionary<string, List<(Items, Vector2)>>();

	// private
	private string levelPath;

	// methods
	public override void _Ready()
	{
		Instance = this;

		// wait for the entire scenetree to be ready (idle frame) before proceeding so that nodes listening
		// to the LevelLoaded signal are loaded before the signal is emitted.
		// level loaded signal when entry level is loaded.
		CallDeferred("emit_signal", nameof(LevelLoaded));
	}

	public void AddItem(string fileName, Items item, Vector2 globalPosition)
	{
		if (DroppedItems.TryGetValue(fileName, out List<(Items, Vector2)> items))
			items.Add((item, globalPosition));
		else
			DroppedItems.Add(fileName, new List<(Items, Vector2)>(1) { (item, globalPosition) });
	}

	public void RemoveItem(string fileName, Items item, Vector2 globalPosition)
	{
		if (DroppedItems.TryGetValue(fileName, out List<(Items, Vector2)> items))
			items.Remove((item, globalPosition));
	}

	public List<(Items, Vector2)> GetDroppedItems(string fileName)
	{
		return DroppedItems.TryGetValue(fileName, out List<(Items, Vector2)> items) ? items : null;
	}

	public Dictionary<string, List<(GlobalSaveManager.ItemData, Vector2)>> GetSaveData()
	{
		Dictionary<string, List<(GlobalSaveManager.ItemData, Vector2)>> keyValuePairs = new();

		foreach (KeyValuePair<string, List<(Items, Vector2)>> keyValuePair in DroppedItems)
		{
			List<(GlobalSaveManager.ItemData, Vector2)> list = new();

			foreach ((Items, Vector2) tuple in keyValuePair.Value)
			{
				list.Add((
					new GlobalSaveManager.ItemData()
					{
						Quantity = 1,
						Path3D = tuple.Item1.ResourcePath
					},
					tuple.Item2
				));
			}

			keyValuePairs.Add(keyValuePair.Key, list);
		}

		return keyValuePairs;
	}

	public void SetSaveData(Dictionary<string, List<(GlobalSaveManager.ItemData, Vector2)>> dictionary)
	{
		DroppedItems.Clear();

		foreach (KeyValuePair<string, List<(GlobalSaveManager.ItemData, Vector2)>> keyValuePair in dictionary)
		{
			List<(Items, Vector2)> list = new();

			foreach ((GlobalSaveManager.ItemData, Vector2) tuple in keyValuePair.Value)
			{
				list.Add((
					GD.Load<Items>(tuple.Item1.Path3D),
					tuple.Item2
				));
			}

			DroppedItems.Add(keyValuePair.Key, list);
		}
	}

	public void ChangeTileMapBounds(Vector2[] newBounds)
	{
		CurrentTileMapBounds = newBounds;
		EmitSignal(nameof(TileMapBoundsChanged), newBounds);
	}

	public void LoadNewLevel(
		string _levelPath,
		string targetTransition,
		Vector2 _positionOffset
	)
	{
		GetTree().Paused = true;
		TargetTransitionArea = targetTransition;
		PositionOffset = _positionOffset;
		levelPath = _levelPath;

		// at this point, the player should be removed from the current level so that the ChangeScene call
		// below wont quefree the player along with the previous level.

		// dont know why but binding string doesnt work so using private levelPath
		GetTree().CreateTimer(SceneTransition.Instance.FadeIn()).Connect(SceneTreeTimer.SignalName.Timeout, new(this, MethodName.LoadNewLevel2));
	}

	private void LoadNewLevel2()
	{
		EmitSignal(nameof(LevelLoadStarted));

		GlobalPlayerManager.Instance.RemovePlayerParent();
		GetTree().ChangeSceneToFile(levelPath);
		GetTree().CreateTimer(SceneTransition.Instance.FadeOut()).Connect(SceneTreeTimer.SignalName.Timeout, new(this, MethodName.LoadNewLevel3));
	}

	private void LoadNewLevel3()
	{
		GetTree().Paused = false;

		EmitSignal(nameof(LevelLoaded));
	}
}
