using Godot;

public class FinishedState : State
{
    // Exports
    [Export]
    private readonly AudioStream exhaustAudio;

    // private
    private AudioStreamPlayer2D audioStreamPlayer2D;

    // methods
    public override void _Ready()
    {
        audioStreamPlayer2D = GetNode<AudioStreamPlayer2D>("../../Audio/AttackSound");
    }

    public override void Enter()
    {
        Player.AnimationPlayer.Play("finished");
        audioStreamPlayer2D.Stream = exhaustAudio;
        audioStreamPlayer2D.Play();
        GlobalAudioManager.Instance.PlayAudio();
        PlayerHUD.Instance.ShowGameOverScreen();
    }

    public override State Process(float delta)
    {
        Player.Velocity = Vector2.Zero;
        return null;
    }
}
