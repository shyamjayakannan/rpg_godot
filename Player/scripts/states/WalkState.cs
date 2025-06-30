using Godot;

public class WalkState : State
{
	// Exports
	[Export]
	private readonly int speed = 100;

	// private
	private IdleState idleState;
	private AttackState attackState;
	private DashState dashState;

	// methods
	public override void _Ready()
	{
		idleState = GetNode<IdleState>("../IdleState");
		attackState = GetNode<AttackState>("../AttackState");
		dashState = GetNode<DashState>("../DashState");
	}

	public override void Enter()
	{
		Player.UpdateAnimation("walk");
	}

	public override State Process(float delta)
	{
		if (Player.SetDirection())
		{
			Player.Velocity = Player.Direction * speed;
			Player.UpdateAnimation("walk");
			return null;
		}

		return idleState;
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
