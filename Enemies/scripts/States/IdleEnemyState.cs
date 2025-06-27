using Godot;

public class IdleEnemyState : EnemyState
{
	// Exports
	[Export]
	private readonly float minStateDuration = 0.5f;
	[Export]
	private readonly float maxStateDuration = 1.5f;

	// private
	private float timer;

	// methods
	public override void Enter()
	{
		Enemy.Velocity = Vector2.Zero;
		Enemy.UpdateAnimation("idle");
		timer = (float)GD.RandRange(minStateDuration, maxStateDuration);
	}

	public override EnemyState Process(float delta)
	{
		timer -= delta;

		if (timer <= 0)
		{
			return NextState;
		}

		return null;
	}
}
