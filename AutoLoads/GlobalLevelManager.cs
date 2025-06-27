using Godot;

public partial class GlobalLevelManager : Node
{
	// Signals
	[Signal]
	public delegate void TileMapBoundsChanged(Vector2[] newBounds);
	[Signal]
	public delegate void LevelLoaded();
	[Signal]
	public delegate void LevelLoadStarted();

	// properties
	public Vector2[] CurrentTileMapBounds { get; private set; }
	public static GlobalLevelManager Instance { get; private set; }
	public string TargetTransitionArea { get; private set; }
	public Vector2 PositionOffset { get; private set; } = Vector2.Zero;

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
		GetTree().CreateTimer(SceneTransition.Instance.FadeIn()).Connect("timeout", this, nameof(LoadNewLevel2));
	}

	private void LoadNewLevel2()
	{
		EmitSignal(nameof(LevelLoadStarted));

		GlobalPlayerManager.Instance.RemovePlayerParent();
		GetTree().ChangeScene(levelPath);
		GetTree().CreateTimer(SceneTransition.Instance.FadeOut()).Connect("timeout", this, nameof(LoadNewLevel3));
	}

	private void LoadNewLevel3()
	{
		GetTree().Paused = false;

		EmitSignal(nameof(LevelLoaded));
	}
}
