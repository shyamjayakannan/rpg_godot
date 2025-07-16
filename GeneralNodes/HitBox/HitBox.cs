using Godot;

namespace Rpg
{
	public partial class HitBox : Area2D
	{
		// Signals
		[Signal]
		public delegate void DamagedEventHandler(HurtBox hurtBox);

		// methods
		public override void _Ready()
		{
			Connect(Area2D.SignalName.AreaEntered, new(this, MethodName.OnHitBoxAreaEntered));
		}

		private void OnHitBoxAreaEntered(Area2D area)
		{
			if (area is HurtBox hurtBox)
			{
				hurtBox.EmitSignal(HurtBox.SignalName.DidDamage);
				EmitSignal(SignalName.Damaged, hurtBox);
			}
		}
	}
}
