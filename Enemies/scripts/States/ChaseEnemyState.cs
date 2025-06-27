using Godot;

public class ChaseEnemyState : EnemyState
{
	// Exports
	[Export]
	private readonly int speed = 100;
	[Export]
	private readonly float stateAnimationDuration = 0.6f;
	[Export]
	private readonly float turnRate = 0.25f;

	// private
	private VisionArea visionArea;
	private HurtBox attackArea;
	private float timer;
	private Vector2 direction;
	private bool canSeePlayer = false;
	private readonly PackedScene pathFinderScene = GD.Load<PackedScene>("res://Enemies/PathFinder.tscn");
	private PathFinder pathFinder;

	// methods
	public override void _Ready()
	{
		base._Ready();

		visionArea = GetNode<VisionArea>("../../VisionArea");
		attackArea = GetNode<HurtBox>("../../Sprite/AttackHurtBox");

		visionArea.Connect(nameof(VisionArea.PlayerEntered), this, nameof(OnPlayerEntered));
		visionArea.Connect(nameof(VisionArea.PlayerExited), this, nameof(OnPlayerExited));
	}

	public override void Enter()
	{
		pathFinder = (PathFinder)pathFinderScene.Instance();
		Enemy.AddChild(pathFinder);
		timer = stateAnimationDuration;
		attackArea.SetDeferred("monitorable", true);
		canSeePlayer = true;
		Enemy.UpdateAnimation("chase");
	}

	public override void Exit()
	{
		pathFinder.QueueFree();
		attackArea.SetDeferred("monitorable", false);
		canSeePlayer = false;
	}

	public override EnemyState Process(float delta)
	{
		if (GlobalPlayerManager.Instance.Player.Hp <= 0)
			return NextState;

		direction = new Vector2(
			Mathf.Lerp(direction.x, pathFinder.BestPath.x, turnRate),
			Mathf.Lerp(direction.y, pathFinder.BestPath.y, turnRate)
		);
		Enemy.Velocity = direction * speed;
		if (Enemy.SetDirection(direction))
			Enemy.UpdateAnimation("chase");

		if (canSeePlayer)
			timer = stateAnimationDuration;
		else
		{
			timer -= delta;

			if (timer <= 0)
				return NextState;
		}

		return null;
	}

	private void OnPlayerEntered()
	{
		canSeePlayer = true;

		if (!(StateMachine.CurrentState is StunEnemyState || StateMachine.CurrentState is DestroyEnemyState))
			StateMachine.ChangeState(this);
	}

	private void OnPlayerExited()
	{
		canSeePlayer = false;
	}
}
