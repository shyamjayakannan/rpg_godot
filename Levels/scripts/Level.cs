using Godot;

public class Level : Node2D
{
	// Exports
	[Export]
	private AudioStream music;

	// methods
	public override void _Ready()
	{
		GlobalAudioManager.Instance.PlayAudio(music);
	}
}
