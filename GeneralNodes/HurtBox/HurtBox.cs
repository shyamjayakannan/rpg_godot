using Godot;

public class HurtBox : Area2D
{
	// Signals
	[Signal]
	public delegate void DidDamage();

	// Exports
	[Export]
	public int Damage { get; set; } = 1;
}
