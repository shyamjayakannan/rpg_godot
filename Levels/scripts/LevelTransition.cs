using Godot;

[Tool]
public partial class LevelTransition : Area2D
{
	// Exports
	[Export(PropertyHint.File, "*.tscn")]
	private string level;
	[Export]
	private SIDE Side
	{
		get => side;
		set
		{
			side = value;

			if (Engine.IsEditorHint())
				UpdateArea();
		}
	}
	[Export]
	private string targetTransitionArea = "LevelTransition";
	[Export(PropertyHint.Range, "1,12,1")]
	private int Size
	{
		get => size;
		set
		{
			size = value;

			if (Engine.IsEditorHint())
				UpdateArea();
		}
	}

	// private
	private SIDE side = SIDE.LEFT;
	private int size = 1;
	private CollisionShape2D collisionShape;

	// public
	public enum SIDE
	{
		LEFT,
		RIGHT,
		TOP,
		BOTTOM
	}

	// methods
	public override void _Ready()
	{
		collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		Connect(Area2D.SignalName.BodyEntered, new(this, MethodName.OnLevelTransitionBodyEntered));

		UpdateArea();

		if (Engine.IsEditorHint())
			return;

		// do this so that the transition doesnt trigger when the level is loaded and the player is standing on the transition area.
		Monitoring = false;
		PlacePlayer();

		// the area2d is still trigerring even after changing player position. only option is to wait till it registers
		GetTree().CreateTimer(0.4f, false).Connect(SceneTreeTimer.SignalName.Timeout, new(this, MethodName.SetMonitoring));
	}

	private void SetMonitoring()
	{
		Monitoring = true;
	}

	protected void UpdateArea()
	{
		Vector2 newRect = new(32, 32);
		Vector2 newPosition = Vector2.Zero;

		switch (side)
		{
			case SIDE.LEFT:
				newRect.Y *= Size;
				newPosition.X -= 16;
				break;
			case SIDE.RIGHT:
				newRect.Y *= Size;
				newPosition.X += 16;
				break;
			case SIDE.TOP:
				newRect.X *= Size;
				newPosition.Y -= 16;
				break;
			case SIDE.BOTTOM:
				newRect.X *= Size;
				newPosition.Y += 16;
				break;
		}

		if (collisionShape != null)
		{
			collisionShape.Shape = new RectangleShape2D
			{
				Size = newRect / 2
			};
			collisionShape.Position = newPosition;
		}
	}

	protected void OnLevelTransitionBodyEntered(Node body = null)
	{
		GlobalLevelManager.Instance.LoadNewLevel(
			level,
			targetTransitionArea,
			GetOffset()
		);
	}

	private void PlacePlayer()
	{
		if (Name != GlobalLevelManager.Instance.TargetTransitionArea)
			return;

		GlobalPlayerManager.Instance.SetPlayerPosition(GlobalPosition + GlobalLevelManager.Instance.PositionOffset);
	}

	private Vector2 GetOffset()
	{
		Vector2 offset = Vector2.Zero;

		if (Side == SIDE.LEFT || Side == SIDE.RIGHT)
		{
			offset.X = (Side == SIDE.LEFT) ? -16 : 16;
			offset.Y = 0;
		}
		else if (Side == SIDE.TOP || Side == SIDE.BOTTOM)
		{
			offset.Y = (Side == SIDE.TOP) ? -16 : 16;
			offset.X = 0;
		}

		return offset;
	}
}
