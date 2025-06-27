using Godot;

public class Enemy : KinematicBody2D
{
	// Signals
	[Signal]
	public delegate void EnemyDamaged(HurtBox hurtBox);
	[Signal]
	public delegate void EnemyDestroyed(HurtBox hurtBox);
	[Signal]
	public delegate void EnemyDirectionChanged(Vector2 newDirection);

	// Exports
	[Export]
	private int hp = 3;
	[Export]
	public int RewardXp { get; private set; } = 1;

	// private
	private Vector2 cardinalDirection = Vector2.Down;
	private Sprite sprite;
	private EnemyStateMachine stateMachine;
	private HitBox hitBox;

	// properties
	public AnimationPlayer AnimationPlayer { get; private set; }
	public bool Invulnerable { get; set; } = false;
	public Vector2 Velocity { get; set; } = Vector2.Zero;

	// methods
	public override void _Ready()
	{
		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		sprite = GetNode<Sprite>("Sprite");
		stateMachine = GetNode<EnemyStateMachine>("EnemyStateMachine");
		hitBox = GetNode<HitBox>("HitBox");
		hitBox.Connect(nameof(HitBox.Damaged), this, nameof(OnHitBoxDamaged));
		stateMachine.Initialize(this);
	}

	public override void _PhysicsProcess(float delta)
	{
		MoveAndSlide(Velocity);
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

		if (direction.x != 0)
			cardinalDirection = direction.x > 0 ? Vector2.Right : Vector2.Left;
		else
			cardinalDirection = direction.y >= 0 ? Vector2.Down : Vector2.Up;

		EmitSignal(nameof(EnemyDirectionChanged), cardinalDirection);

		if (cardinalDirection.x < 0)
			sprite.Scale = new Vector2(-1, 1);
		else
			sprite.Scale = new Vector2(1, 1);

		return true;
	}

	private void OnHitBoxDamaged(HurtBox hurtBox)
	{
		if (Invulnerable)
			return;

		hp -= hurtBox.Damage;
		GlobalEffectManager.Instance.DamageTexter(hurtBox.Damage.ToString(), GlobalPosition + new Vector2(0, -40));

		if (hp > 0)
			EmitSignal(nameof(EnemyDamaged), hurtBox);
		else
			EmitSignal(nameof(EnemyDestroyed), hurtBox);
	}
}
