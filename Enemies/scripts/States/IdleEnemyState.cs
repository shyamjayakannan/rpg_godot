using Godot;

namespace Rpg
{
	public partial class IdleEnemyState : EnemyState
	{
		// Exports
		[Export]
		private float minStateDuration = 0.5f;
		[Export]
		private float maxStateDuration = 1.5f;

		// private
		private float timer;

		// methods
		public override void Enter()
		{
			Enemy.Velocity = Vector2.Zero;
			Enemy.UpdateAnimation("idle");
			timer = (float)GD.RandRange(minStateDuration, maxStateDuration);
		}

		public override EnemyState Process(double delta)
		{
			timer -= (float)delta;

			if (timer <= 0)
			{
				return NextState;
			}

			return null;
		}
	}
}
