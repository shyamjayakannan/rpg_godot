using Godot;

public class WanderEnemyState : EnemyState
{
	// Exports
	[Export]
	private readonly int speed = 100;
	[Export]
	private readonly float stateAnimationDuration = 0.6f;
	[Export]
	private readonly int minCycles = 1;
	[Export]
	private readonly int maxCycles = 3;

	// private
	private float timer;
	private Vector2 direction;

	// methods
	public override void Enter()
	{
		timer = (GD.Randi() % (maxCycles - minCycles + 1) + minCycles) * stateAnimationDuration;
		direction = new Vector2[]{
			Vector2.Left,
			Vector2.Right,
			Vector2.Up,
			Vector2.Down,
		}[GD.Randi() % 4];
		Enemy.SetDirection(direction);
		Enemy.Velocity = direction * speed;
		Enemy.UpdateAnimation("walk");
	}

	public override EnemyState Process(float delta)
	{
		timer -= delta;

		if (timer <= 0)
			return NextState;

		return null;
	}
}
