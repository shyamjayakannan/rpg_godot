using Godot;

public class Plant : Node2D
{
	// methods
	public override void _Ready()
	{
		GetNode<HitBox>("HitBox").Connect(nameof(HitBox.Damaged), this, nameof(OnHitBoxDamaged));
	}

	private void OnHitBoxDamaged(int damage)
	{
		QueueFree();
	}
}
