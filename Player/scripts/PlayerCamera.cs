using Godot;

public class PlayerCamera : Camera2D
{
	// methods
	public override void _Ready()
	{
		GlobalLevelManager.Instance.Connect(nameof(GlobalLevelManager.TileMapBoundsChanged), this, nameof(OnTileMapBoundsChanged));

		// this call is needed because the TileMapBoundsChanged signal, the first time, is emmitted before PlayerCamera is ready
		// so we need to manually set the camera limits based on the current tile map bounds
		OnTileMapBoundsChanged(GlobalLevelManager.Instance.CurrentTileMapBounds);
	}

	private void OnTileMapBoundsChanged(Vector2[] newBounds)
	{
		if (newBounds == null)
			return;

		// Set the camera limits based on the new bounds
		LimitLeft = (int)newBounds[0].x;
		LimitTop = (int)newBounds[0].y;
		LimitRight = (int)newBounds[1].x;
		LimitBottom = (int)newBounds[1].y;
	}
}
