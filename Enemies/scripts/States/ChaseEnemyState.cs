using Godot;

namespace Rpg
{
	public partial class ChaseEnemyState : EnemyState
	{
		// Exports
		[Export]
		private int speed = 100;
		[Export]
		private float stateAnimationDuration = 0.6f;
		[Export]
		private float turnRate = 0.25f;

		// private
		private VisionArea visionArea;
		private HurtBox attackArea;
		private float timer;
		private Vector2 direction;
		private bool canSeePlayer = false;
		private PackedScene pathFinderScene = GD.Load<PackedScene>("res://Enemies/PathFinder.tscn");
		private PathFinder pathFinder;

		// methods
		public override void _Ready()
		{
			base._Ready();

			visionArea = GetNode<VisionArea>("../../VisionArea");
			attackArea = GetNode<HurtBox>("../../Sprite2D/AttackHurtBox");

			visionArea.Connect(VisionArea.SignalName.PlayerEntered, new(this, MethodName.OnPlayerEntered));
			visionArea.Connect(VisionArea.SignalName.PlayerExited, Callable.From(() => canSeePlayer = false));
		}

		public override void Enter()
		{
			pathFinder = (PathFinder)pathFinderScene.Instantiate();
			Enemy.AddChild(pathFinder);
			timer = stateAnimationDuration;
			attackArea.SetDeferred(Area2D.PropertyName.Monitorable, true);
			canSeePlayer = true;
			Enemy.UpdateAnimation("chase");
		}

		public override void Exit()
		{
			pathFinder.QueueFree();
			attackArea.SetDeferred(Area2D.PropertyName.Monitorable, false);
			canSeePlayer = false;
		}

		public override EnemyState Process(double delta)
		{
			if (GlobalPlayerManager.Instance.Player.Hp <= 0)
				return NextState;

			direction = new(
				Mathf.Lerp(direction.X, pathFinder.BestPath.X, turnRate),
				Mathf.Lerp(direction.Y, pathFinder.BestPath.Y, turnRate)
			);
			Enemy.Velocity = direction * speed;
			if (Enemy.SetDirection(direction))
				Enemy.UpdateAnimation("chase");

			if (canSeePlayer)
				timer = stateAnimationDuration;
			else
			{
				timer -= (float)delta;

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
	}
}
