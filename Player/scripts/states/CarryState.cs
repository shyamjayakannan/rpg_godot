using Godot;

public class CarryState : State
{
    // Exports
    [Export]
    private readonly int speed = 100;
    [Export]
    private readonly AudioStream throwAudio;

    // private
    private IdleState idleState;
    private StunState stunState;
    private AudioStreamPlayer2D audioStreamPlayer2D;

    // methods
    public override void _Ready()
    {
        idleState = GetNode<IdleState>("../IdleState");
        stunState = GetNode<StunState>("../StunState");
        audioStreamPlayer2D = GetNode<AudioStreamPlayer2D>("../../Audio/AttackSound");
    }

    public override void Enter()
    {
        Player.UpdateAnimation("carry");
    }

    public override void Exit()
    {
        if (StateMachine.NextState == stunState)
        {
            Player.Throwable.SetState("drop", Vector2.Zero);
            return;
        }

        audioStreamPlayer2D.Stream = throwAudio;
        audioStreamPlayer2D.Play();
        Player.Throwable.SetState("throw", Player.Direction == Vector2.Zero ? Player.CardinalDirection : Player.Direction);
    }

    public override State Process(float delta)
    {
        Player.Velocity = Player.Direction * speed;

        if (Player.SetDirection())
        {
            Player.UpdateAnimation("carryWalk");
            return null;
        }

        Player.UpdateAnimation("carry");

        return null;
    }

    public override State HandleInput(InputEvent _event)
    {
        if (_event.IsActionPressed("interact"))
            return idleState;

        return null;
    }
}
