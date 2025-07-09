using Godot;

public partial class LiftState : State
{
    // Exports
    [Export]
    private AudioStream audioStream;

    // private
    private CarryState carryState;
    private AudioStreamPlayer2D audioStreamPlayer2D;
    private bool startLate = false;

    // methods
    public override void _Ready()
    {
        carryState = GetNode<CarryState>("../CarryState");
        audioStreamPlayer2D = GetNode<AudioStreamPlayer2D>("../../Audio/AttackSound");
    }

    public override void Enter()
    {
        Player.UpdateAnimation("lift");

        if (startLate)
            Player.AnimationPlayer.Seek(0.19f); // just before end of lift animation (immediate carry for bomb)

        Player.AnimationPlayer.Connect(AnimationMixer.SignalName.AnimationFinished, Callable.From(() => StateMachine.ChangeState(carryState)), (uint)ConnectFlags.OneShot);
        audioStreamPlayer2D.Stream = audioStream;
        audioStreamPlayer2D.Play();
    }

    public override void Exit()
    {
        startLate = false;
    }

    public void SetStartLate(bool value)
    {
        startLate = value;
    }
}
