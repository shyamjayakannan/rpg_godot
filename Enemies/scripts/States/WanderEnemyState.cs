using Godot;

public partial class WanderEnemyState : EnemyState
{
	// Exports
	[Export]
	private int speed = 100;
	[Export]
	private float stateAnimationDuration = 0.6f;
	[Export]
	private int minCycles = 1;
	[Export]
	private int maxCycles = 3;

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

	public override EnemyState Process(double delta)
	{
		timer -= (float)delta;

		if (timer <= 0)
			return NextState;

		return null;
	}
}
