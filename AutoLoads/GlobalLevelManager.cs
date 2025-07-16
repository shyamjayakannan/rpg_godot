using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Rpg
{
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
		public Dictionary<string, List<(Items, Vector2, int)>> DroppedItems { get; set; } = new();

		// private
		private string levelPath;

		// methods
		public override void _Ready()
		{
			Instance = this;

			// wait for the entire scenetree to be ready (idle frame) before proceeding so that nodes listening
			// to the LevelLoaded signal are loaded before the signal is emitted.
			// level loaded signal when entry level is loaded.
			CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.LevelLoaded);
		}

		public void AddItem(string fileName, Items item, Vector2 globalPosition, int ySortOrigin)
		{
			if (DroppedItems.TryGetValue(fileName, out List<(Items, Vector2, int)> items))
				items.Add((item, globalPosition, ySortOrigin));
			else
				DroppedItems.Add(fileName, new(1) { (item, globalPosition, ySortOrigin) });
		}

		public void RemoveItem(string fileName, Items item, Vector2 globalPosition, int ySortOrigin)
		{
			if (DroppedItems.TryGetValue(fileName, out List<(Items, Vector2, int)> items))
				items.Remove((item, globalPosition, ySortOrigin));
		}

		public List<(Items, Vector2, int)> GetDroppedItems(string fileName)
		{
			return DroppedItems.TryGetValue(fileName, out List<(Items, Vector2, int)> items) ? items : null;
		}

		public Dictionary<string, List<(GlobalSaveManager.ItemData, Vector2, int)>> GetSaveData()
		{
			return DroppedItems.ToDictionary(
				entry => entry.Key,
				entry => entry.Value.Select(tuple => (
					new GlobalSaveManager.ItemData
					{
						Quantity = 1,
						Path = tuple.Item1.ResourcePath
					},
					tuple.Item2,
					tuple.Item3
				)).ToList()
			);
		}

		public void SetSaveData(Dictionary<string, List<(GlobalSaveManager.ItemData, Vector2, int)>> dictionary)
		{
			DroppedItems.Clear();

			DroppedItems = dictionary.ToDictionary(
				entry => entry.Key,
				entry => entry.Value.Select(tuple => (
					GD.Load<Items>(tuple.Item1.Path),
					tuple.Item2,
					tuple.Item3
				)).ToList()
			);
		}

		public void ChangeTileMapBounds(Vector2[] newBounds)
		{
			CurrentTileMapBounds = newBounds;
			EmitSignal(SignalName.TileMapBoundsChanged, newBounds);
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
			EmitSignal(SignalName.LevelLoadStarted);

			GlobalPlayerManager.Instance.RemovePlayerParent();
			GetTree().ChangeSceneToFile(levelPath);
			GetTree().CreateTimer(SceneTransition.Instance.FadeOut()).Connect(SceneTreeTimer.SignalName.Timeout, new(this, MethodName.LoadNewLevel3));
		}

		private void LoadNewLevel3()
		{
			GetTree().Paused = false;

			EmitSignal(SignalName.LevelLoaded);
		}
	}
}
