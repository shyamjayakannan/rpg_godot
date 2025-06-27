using Godot;

public class PlayerSpawn : Node2D
{
	// methods
	public override void _Ready()
	{
		Hide();

		if (!GlobalPlayerManager.Instance.PlayerSpawned)
			GlobalPlayerManager.Instance.SetPlayerPosition(GlobalPosition);
	}
}
