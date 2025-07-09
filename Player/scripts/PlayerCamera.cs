using Godot;

public partial class PlayerCamera : Camera2D
{
	// methods
	public override void _Ready()
	{
		GlobalLevelManager.Instance.Connect(GlobalLevelManager.SignalName.TileMapBoundsChanged, new(this, MethodName.OnTileMapBoundsChanged));

		// this call is needed because the TileMapBoundsChanged signal, the first time, is emmitted before PlayerCamera is ready
		// so we need to manually set the camera limits based on the current tile map bounds
		OnTileMapBoundsChanged(GlobalLevelManager.Instance.CurrentTileMapBounds);
	}

	private void OnTileMapBoundsChanged(Vector2[] newBounds)
	{
		if (newBounds == null)
			return;

		// Set the camera limits based on the new bounds
		LimitLeft = (int)newBounds[0].X;
		LimitTop = (int)newBounds[0].Y;
		LimitRight = (int)newBounds[1].X;
		LimitBottom = (int)newBounds[1].Y;
	}
}
