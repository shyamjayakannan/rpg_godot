using System.Linq;
using Godot;

public class Player : KinematicBody2D
{
	// Signals
	[Signal]
	public delegate void AttackAnimationOver();
	[Signal]
	public delegate void PlayerDamaged(HurtBox hurtBox);
	[Signal]
	public delegate void PlayerDirectionChanged(Vector2 newDirection);

	// private
	private AnimationPlayer attackAnimationPlayer;
	private HurtBox attackHurtBox;
	private HurtBox chargeAttackHurtBox;
	public Vector2 CardinalDirection { get; private set; } = Vector2.Down;
	private PlayerStateMachine stateMachine;
	private bool invulnerable = false;
	private HitBox hitBox;
	private Node2D heldItems;
	private LiftState liftState;
	private IdleState idleState;
	private int attack = 1;

	// properties
	public Sprite Sprite { get; private set; }
	public int[] LevelUpXpRequirements { get; private set; } = new int[4] { 0, 50, 100, 200 };
	public AnimationPlayer AnimationPlayer { get; private set; }
	public int Hp { get; private set; } = 4;
	public int MaxHp { get; private set; } = 6;
	public int Level { get; set; } = 1;
	public int Xp { get; set; } = 0;
	public int Defence { get; set; } = 1;
	public int Attack
	{
		get => attack;
		set
		{
			attack = value;

			if (attackHurtBox != null)
				attackHurtBox.Damage = attack;

			if (chargeAttackHurtBox != null)
				chargeAttackHurtBox.Damage = attack * 2;
		}
	}
	public int Bombs = 10;
	public int Arrows = 0;
	public Vector2 Direction { get; private set; } = Vector2.Zero;
	public Vector2 Velocity { get; set; } = Vector2.Zero;
	public AnimationPlayer EffectAnimationPlayer { get; private set; }
	public Throwable Throwable { get; set; }

	// methods
	public override void _Ready()
	{
		UpdateHP(MaxHp);

		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		attackAnimationPlayer = GetNode<AnimationPlayer>("Sprite/AttackEffects/AnimationPlayer");
		EffectAnimationPlayer = GetNode<AnimationPlayer>("EffectAnimationPlayer");
		Sprite = GetNode<Sprite>("Sprite");
		attackHurtBox = Sprite.GetNode<HurtBox>("HurtBox");
		chargeAttackHurtBox = Sprite.GetNode<HurtBox>("ChargeHurtBox");
		stateMachine = GetNode<PlayerStateMachine>("PlayerStateMachine");
		liftState = GetNode<LiftState>("PlayerStateMachine/LiftState");
		idleState = GetNode<IdleState>("PlayerStateMachine/IdleState");
		hitBox = GetNode<HitBox>("HitBox");
		heldItems = GetNode<Node2D>("Sprite/HeldItems");

		hitBox.Connect(nameof(HitBox.Damaged), this, nameof(OnHitBoxDamaged));
		AnimationPlayer.Connect("animation_finished", this, nameof(OnAnimationPlayerAnimationFinished));
		GlobalLevelManager.Instance.Connect(nameof(GlobalLevelManager.LevelLoaded), this, nameof(OnLevelLoaded));

		stateMachine.Initialize(this);
	}

	private void OnLevelLoaded()
	{
		// VERY IMPORTANT
		// needed for level transition direction reset on new level load
		if (!(Input.IsActionPressed("ui_right") || Input.IsActionPressed("ui_left") || Input.IsActionPressed("ui_down") || Input.IsActionPressed("ui_up")))
			Direction = Vector2.Zero;
	}

	private void OnAnimationPlayerAnimationFinished(string animationName)
	{
		if (animationName == "attackDown" || animationName == "attackUp" || animationName == "attackSide")
			EmitSignal(nameof(AttackAnimationOver));
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		Direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
	}

	public override void _PhysicsProcess(float delta)
	{
		MoveAndSlide(Velocity, Vector2.Up, false, 4, Mathf.Pi / 4, false);

		if (GetSlideCount() == 0)
			return;

		KinematicCollision2D collision = GetSlideCollision(0);

		if (collision.Collider is RigidBody2D body2D)
			body2D.ApplyCentralImpulse(Direction * 1000.0f);
	}

	public void UpdateAnimation(string state)
	{
		AnimationPlayer.Play($"{state}{AnimationDirection()}");

		if (state == "attack")
			attackAnimationPlayer.Play($"attack{AnimationDirection()}");
	}

	private string AnimationDirection()
	{
		if (CardinalDirection == Vector2.Down)
			return "Down";
		else if (CardinalDirection == Vector2.Up)
			return "Up";

		return "Side";
	}

	public bool SetDirection()
	{
		if (Direction == Vector2.Zero)
			return false;

		if (Direction.x != 0)
			CardinalDirection = Direction.x > 0 ? Vector2.Right : Vector2.Left;
		else
			CardinalDirection = Direction.y >= 0 ? Vector2.Down : Vector2.Up;

		EmitSignal(nameof(PlayerDirectionChanged), CardinalDirection);

		if (CardinalDirection.x < 0)
			Sprite.Scale = new Vector2(-1, 1);
		else
			Sprite.Scale = new Vector2(1, 1);

		return true;
	}

	private void OnHitBoxDamaged(HurtBox hurtBox)
	{
		if (invulnerable)
			return;

		int oldHp = Hp;
		UpdateHP(-Mathf.Clamp(hurtBox.Damage - Defence, 1, hurtBox.Damage));

		if (oldHp > 0)
			EmitSignal(nameof(PlayerDamaged), hurtBox);
	}

	public void UpdateHP(int delta)
	{
		Hp = Mathf.Clamp(Hp + delta, 0, MaxHp);
		PlayerHUD.Instance.UpdateHP(Hp, MaxHp);
	}

	public void UpdateXP(int delta)
	{
		Xp = Mathf.Clamp(Xp + delta, 0, LevelUpXpRequirements.Last());

		while (Level < LevelUpXpRequirements.Length && Xp > LevelUpXpRequirements[Level])
		{
			Level++;
			Attack++;
			Defence++;
			EffectAnimationPlayer.Queue("levelUp");
			UpdateHP(MaxHp);
			PlayerHUD.Instance.QueueNotification("Leveled Up!", $"You reached Level {Level}");
		}

		if (Level == LevelUpXpRequirements.Length)
			Level--;
	}

	public void SetHP(int hp, int maxHp)
	{
		MaxHp = maxHp;
		UpdateHP(hp);
	}

	public void MakeInvulnerable(float stunDuration)
	{
		invulnerable = true;
		SetDeferred("hitBox.Monitoring", false);
		GetTree().CreateTimer(stunDuration, false).Connect("timeout", this, nameof(MakeInvulnerable2));
	}

	private void MakeInvulnerable2()
	{
		invulnerable = false;
		SetDeferred("hitBox.Monitoring", true);
	}

	public void PickupItem(Throwable throwable, Node2D throwableParent)
	{
		// reset position
		throwableParent.Position = Vector2.Zero;
		Throwable = throwable;
		heldItems.AddChild(throwableParent);
		stateMachine.ChangeState(liftState);
	}

	public void RevivePlayer()
	{
		UpdateHP(MaxHp);
		stateMachine.ChangeState(idleState);
	}

	public void ChangeStateToIdle()
	{
		Direction = Vector2.Zero;
	}

	public State GetCurrentState()
	{
		return stateMachine.GetCurrentState();
	}
}
