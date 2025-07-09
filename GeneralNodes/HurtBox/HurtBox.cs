using Godot;

public partial class HurtBox : Area2D
{
	// Signals
	[Signal]
	public delegate void DidDamageEventHandler();

	// Exports
	[Export]
	public int Damage { get; set; } = 1;
}
