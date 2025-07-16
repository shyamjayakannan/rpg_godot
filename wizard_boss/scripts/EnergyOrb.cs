using Godot;

namespace Rpg
{
	public partial class EnergyOrb : Node2D
	{
		// Exports
		[Export]
		private float speed = 100;
		[Export]
		private AudioStream shootSound;
		[Export]
		private AudioStream hitSound;

		// private
		private Vector2 direction = Vector2.Down;
		private HurtBox hurtBox;
		private AudioStreamPlayer2D audioStreamPlayer2D;

		public override void _Ready()
		{
			hurtBox = GetNode<HurtBox>("HurtBox");
			audioStreamPlayer2D = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");

			hurtBox.Connect(HurtBox.SignalName.DidDamage, new(this, MethodName.OnHurtBoxDidDamage));
			PlayAudio(shootSound);
			direction = GlobalPosition.DirectionTo(GlobalPlayerManager.Instance.Player.GlobalPosition);
			GetTree().CreateTimer(4, false).Connect(SceneTreeTimer.SignalName.Timeout, new(this, MethodName.Destroy));
		}

		public override void _Process(double delta)
		{
			Position += direction * speed * (float)delta;
		}

		private void Destroy()
		{
			SetProcess(false);
			QueueFree();
		}

		// signal callbacks cannot be async, so we use an async void method inside
		private void OnHurtBoxDidDamage()
		{
			PlayAudio(hitSound);
			Hide();
			SetProcess(false);
			audioStreamPlayer2D.Connect(AudioStreamPlayer2D.SignalName.Finished, new(this, Node.MethodName.QueueFree));
		}

		private void PlayAudio(AudioStream stream)
		{
			audioStreamPlayer2D.Stream = stream;
			audioStreamPlayer2D.Play();
		}
	}
}
