using Godot;

public partial class Plant : Node2D
{
	// methods
	public override void _Ready()
	{
		GetNode<HitBox>("HitBox").Connect(HitBox.SignalName.Damaged, new(this, Node.MethodName.QueueFree));
	}
}
