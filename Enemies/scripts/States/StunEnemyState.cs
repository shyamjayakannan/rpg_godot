using Godot;

public class StunEnemyState : EnemyState
{
	// Exports
	[Export]
	private readonly float knockbackSpeed = 200f;
	[Export]
	private readonly float deceleration = 5;

	// private
	private Vector2 direction;
	private bool animationFinished = false;
	private Vector2 damagePosition;

	// methods
	public override void Init()
	{
		Enemy.Connect(nameof(Enemy.EnemyDamaged), this, nameof(OnEnemyDamaged));
	}

	public override void Enter()
	{
		animationFinished = false;
		direction = Enemy.GlobalPosition.DirectionTo(damagePosition);
		Enemy.SetDirection(direction);
		Enemy.Velocity = direction * (-knockbackSpeed);
		Enemy.UpdateAnimation("stun");
		Enemy.AnimationPlayer.Connect("animation_finished", this, nameof(OnAnimationPlayerAnimationFinished));
		Enemy.Invulnerable = true;
	}

	public override void Exit()
	{
		Enemy.AnimationPlayer.Disconnect("animation_finished", this, nameof(OnAnimationPlayerAnimationFinished));
		Enemy.Invulnerable = false;
	}

	public override EnemyState Process(float delta)
	{
		if (animationFinished)
			return NextState;

		Enemy.Velocity *= 1 - deceleration * delta;

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
