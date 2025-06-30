using Godot;

public class IdleState : State
{
	// private
	private WalkState walkState;
	private AttackState attackState;
	private DashState dashState;

	// methods
	public override void _Ready()
	{
		walkState = GetNode<WalkState>("../WalkState");
		attackState = GetNode<AttackState>("../AttackState");
		dashState = GetNode<DashState>("../DashState");
	}

	public override void Enter()
	{
		Player.UpdateAnimation("idle");
	}

	public override State Process(float delta)
	{
		if (Player.Direction != Vector2.Zero)
			return walkState;

		Player.Velocity = Vector2.Zero;
		return null;
	}

	public override State HandleInput(InputEvent _event)
	{
		if (_event.IsActionPressed("attack"))
			return attackState;

		if (_event.IsActionPressed("dash"))
			return dashState;

		if (_event.IsActionPressed("interact"))
			GlobalPlayerManager.Instance.EmitSignal(nameof(GlobalPlayerManager.InteractPressed));

		return null;
	}
}
