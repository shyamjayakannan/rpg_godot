using Godot;

public partial class Plant : StaticBody2D
{
	// methods
	public override void _Ready()
	{
		GetNode<HitBox>("HitBox").Connect(HitBox.SignalName.Damaged, Callable.From((HurtBox _) => GetParent().QueueFree()));
	}
}
