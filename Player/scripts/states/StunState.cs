using Godot;

namespace Rpg
{
	public partial class StunState : State
	{
		// Exports
		[Export]
		private float stunDuration = 1.0f;
		[Export]
		private float knockbackSpeed = 200f;
		[Export]
		private float deceleration = 5;

		// private
		private Vector2 direction;
		private HurtBox hurtBox;
		private IdleState idleState;
		private FinishedState finishedState;
		private State nextState;
		private AnimationPlayer animationPlayer;

		// methods
		public override void _Ready()
		{
			idleState = GetNode<IdleState>("../IdleState");
			finishedState = GetNode<FinishedState>("../FinishedState");
			animationPlayer = GetNode<AnimationPlayer>("../../AnimationPlayer");
		}

		public override void Init()
		{
			Player.Connect(Player.SignalName.PlayerDamaged, new(this, MethodName.OnPlayerPlayerDamaged));
		}

		public override void Enter()
		{
			direction = Player.GlobalPosition.DirectionTo(hurtBox.GlobalPosition);
			Player.Velocity = direction * (-knockbackSpeed);
			Player.SetDirection();

			// do these after setting the velocity and direction to avoid flickering
			Player.UpdateAnimation("stun");
			Player.EffectAnimationPlayer.Play("default");
			animationPlayer.Connect(AnimationMixer.SignalName.AnimationFinished, new(this, MethodName.OnAnimationPlayerAnimationFinished));
			Player.MakeInvulnerable(stunDuration);
		}

		public override void Exit()
		{
			animationPlayer.Disconnect(AnimationMixer.SignalName.AnimationFinished, new(this, MethodName.OnAnimationPlayerAnimationFinished));
			nextState = null;
		}

		public override State Process(float delta)
		{
			Player.Velocity *= 1 - deceleration * delta;
			return nextState;
		}

		private void OnPlayerPlayerDamaged(HurtBox _hurtBox)
		{
			hurtBox = _hurtBox;
			StateMachine.ChangeState(this);
		}

		private void OnAnimationPlayerAnimationFinished(string _)
		{
			if (Player.Hp > 0)
				nextState = idleState;
			else
				nextState = finishedState;
		}
	}
}
