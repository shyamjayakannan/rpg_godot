using Godot;

public partial class StunEnemyState : EnemyState
{
	// Exports
	[Export]
	private float knockbackSpeed = 200f;
	[Export]
	private float deceleration = 5;

	// private
	private Vector2 direction;
	private bool animationFinished = false;
	private Vector2 damagePosition;

	// methods
	public override void Init()
	{
		Enemy.Connect(Enemy.SignalName.EnemyDamaged, new(this, MethodName.OnEnemyDamaged));
	}

	public override void Enter()
	{
		animationFinished = false;
		direction = Enemy.GlobalPosition.DirectionTo(damagePosition);
		Enemy.SetDirection(direction);
		Enemy.Velocity = direction * (-knockbackSpeed);
		Enemy.UpdateAnimation("stun");
		Enemy.AnimationPlayer.Connect(AnimationMixer.SignalName.AnimationFinished, new(this, MethodName.OnAnimationPlayerAnimationFinished));
		Enemy.Invulnerable = true;
	}

	public override void Exit()
	{
		Enemy.AnimationPlayer.Disconnect(AnimationMixer.SignalName.AnimationFinished, new(this, nameof(OnAnimationPlayerAnimationFinished)));
		Enemy.Invulnerable = false;
	}

	public override EnemyState Process(double delta)
	{
		if (animationFinished)
			return NextState;

		Enemy.Velocity *= 1 - deceleration * (float)delta;

		return null;
	}

	private void OnEnemyDamaged(HurtBox hurtBox)
	{
		damagePosition = hurtBox.GlobalPosition;
		StateMachine.ChangeState(this);
	}

	private void OnAnimationPlayerAnimationFinished(string animName)
	{
		animationFinished = true;
	}
}
