using Godot;

namespace Rpg
{
	public partial class Enemy : CharacterBody2D
	{
		// Signals
		[Signal]
		public delegate void EnemyDamagedEventHandler(HurtBox hurtBox);
		[Signal]
		public delegate void EnemyDestroyedEventHandler(HurtBox hurtBox);
		[Signal]
		public delegate void EnemyDirectionChangedEventHandler(Vector2 newDirection);

		// Exports
		[Export]
		private int hp = 3;
		[Export]
		public int RewardXp { get; private set; } = 1;

		// private
		private Vector2 cardinalDirection = Vector2.Down;
		private Sprite2D sprite;
		private EnemyStateMachine stateMachine;
		private HitBox hitBox;

		// properties
		public AnimationPlayer AnimationPlayer { get; private set; }
		public bool Invulnerable { get; set; } = false;

		// methods
		public override void _Ready()
		{
			AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
			sprite = GetNode<Sprite2D>("Sprite2D");
			stateMachine = GetNode<EnemyStateMachine>("EnemyStateMachine");
			hitBox = GetNode<HitBox>("HitBox");
			hitBox.Connect(HitBox.SignalName.Damaged, new(this, MethodName.OnHitBoxDamaged));
			stateMachine.Initialize(this);
		}

		public override void _PhysicsProcess(double delta)
		{
			MoveAndSlide();
		}

		public void UpdateAnimation(string state)
		{
			AnimationPlayer.Play($"{state}{AnimationDirection()}");
		}

		private string AnimationDirection()
		{
			if (cardinalDirection == Vector2.Down)
				return "Down";
			else if (cardinalDirection == Vector2.Up)
				return "Up";

			return "Side";
		}

		public bool SetDirection(Vector2 direction)
		{
			if (direction == Vector2.Zero)
				return false;

			if (direction.X != 0)
				cardinalDirection = direction.X > 0 ? Vector2.Right : Vector2.Left;
			else
				cardinalDirection = direction.Y >= 0 ? Vector2.Down : Vector2.Up;

			EmitSignal(SignalName.EnemyDirectionChanged, cardinalDirection);

			if (cardinalDirection.X < 0)
				sprite.Scale = new(-1, 1);
			else
				sprite.Scale = new(1, 1);

			return true;
		}

		private void OnHitBoxDamaged(HurtBox hurtBox)
		{
			if (Invulnerable)
				return;

			hp -= hurtBox.Damage;
			GlobalEffectManager.Instance.DamageTexter(hurtBox.Damage.ToString(), GlobalPosition + new Vector2(0, -40));

			if (hp > 0)
				EmitSignal(SignalName.EnemyDamaged, hurtBox);
			else
				EmitSignal(SignalName.EnemyDestroyed, hurtBox);
		}
	}
}
