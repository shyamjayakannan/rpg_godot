using Godot;

public class AttackState : State
{
	// Exports
	[Export(PropertyHint.Range, "0.1, 10.0")]
	private readonly float deceleration = 5f;
	[Export]
	private readonly AudioStream audioStream;

	// private
	private IdleState idleState;
	private WalkState walkState;
	private ChargeState chargeState;
	private bool isAttacking = false;
	private AudioStreamPlayer2D attackSound;
	private HurtBox hurtBox;

	// methods
	public override void _Ready()
	{
		idleState = GetNode<IdleState>("../IdleState");
		walkState = GetNode<WalkState>("../WalkState");
		chargeState = GetNode<ChargeState>("../ChargeState");
		attackSound = GetNode<AudioStreamPlayer2D>("../../Audio/AttackSound");
		hurtBox = GetNode<HurtBox>("../../Sprite/HurtBox");
	}

	public override void Enter()
	{
		Player.UpdateAnimation("attack");
		Player.Connect(nameof(Player.AttackAnimationOver), this, nameof(OnPlayerAttackAnimationOver));
		attackSound.Stream = audioStream;
		attackSound.Play();
		isAttacking = true;

		// wait for sword to reach middle before enabling hurtbox
		GetTree().CreateTimer(0.2f, false).Connect("timeout", this, nameof(Enter2));
	}

	private void Enter2()
	{
		// the player may enter stun state if an enemy attacks before the above timer finishes.
		// in this case, the exit function will be called before the below line can run.
		// so the hurtbox.monitorable will get set to true in such cases instead of false (as the exit
		// function should normally run last but the below line runs last in this case)
		// so check if isattacking is true before doing this (this ensures that the below line wont run
		// if exit has already run)
		if (isAttacking)
			hurtBox.Monitorable = true;
	}

	public override void Exit()
	{
		Player.Disconnect(nameof(Player.AttackAnimationOver), this, nameof(OnPlayerAttackAnimationOver));
		hurtBox.SetDeferred("monitorable", false);
		isAttacking = false;
	}

	public override State Process(float delta)
	{
		if (!isAttacking)
		{
			if (Player.Velocity != Vector2.Zero)
				return walkState;
			else
				return idleState;
		}

		Player.Velocity -= Player.Velocity * deceleration * delta;
		return null;
	}

	private void OnPlayerAttackAnimationOver()
	{
		if (Input.IsActionPressed("attack"))
			StateMachine.ChangeState(chargeState);

		isAttacking = false;
	}
}
