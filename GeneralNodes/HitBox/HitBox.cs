using Godot;

public class HitBox : Area2D
{
	// Signals
	[Signal]
	public delegate void Damaged(HurtBox hurtBox);

	// methods
	public override void _Ready()
	{
		Connect("area_entered", this, nameof(OnHitBoxAreaEntered));
	}

	private void OnHitBoxAreaEntered(Area2D area)
	{
		if (area is HurtBox hurtBox)
		{
			hurtBox.EmitSignal(nameof(HurtBox.DidDamage));
			EmitSignal(nameof(Damaged), hurtBox);
		}
	}
}
