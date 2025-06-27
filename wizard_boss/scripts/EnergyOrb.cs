using Godot;

public class EnergyOrb : Node2D
{
	// Exports
	[Export]
	private readonly float speed = 100;
	[Export]
	private readonly AudioStream shootSound;
	[Export]
	private readonly AudioStream hitSound;

	// private
	private Vector2 direction = Vector2.Down;
	private HurtBox hurtBox;
	private AudioStreamPlayer2D audioStreamPlayer2D;

	public override void _Ready()
	{
		hurtBox = GetNode<HurtBox>("HurtBox");
		audioStreamPlayer2D = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");

		hurtBox.Connect(nameof(HurtBox.DidDamage), this, nameof(OnHurtBoxDidDamage));
		PlayAudio(shootSound);
		direction = GlobalPosition.DirectionTo(GlobalPlayerManager.Instance.Player.GlobalPosition);
		GetTree().CreateTimer(4, false).Connect("timeout", this, nameof(Destroy));
	}

	public override void _Process(float delta)
	{
		Position += direction * speed * delta;
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
		audioStreamPlayer2D.Connect("finished", this, nameof(OnHurtBoxDidDamage2));
	}

	private void OnHurtBoxDidDamage2()
	{
		QueueFree();
	}

	private void PlayAudio(AudioStream stream)
	{
		audioStreamPlayer2D.Stream = stream;
		audioStreamPlayer2D.Play();
	}
}
