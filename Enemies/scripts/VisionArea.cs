using Godot;

public partial class VisionArea : Area2D
{
    // Signals
    [Signal]
    public delegate void PlayerEnteredEventHandler();
    [Signal]
    public delegate void PlayerExitedEventHandler();

    // methods
    public override void _Ready()
    {
        Connect(Area2D.SignalName.BodyEntered, new(this, MethodName.OnVisionAreaBodyEntered));
        Connect(Area2D.SignalName.BodyExited, new(this, MethodName.OnVisionAreaBodyExited));

        if (GetParent() is Enemy parent)
            parent.Connect(Enemy.SignalName.EnemyDirectionChanged, new(this, MethodName.OnEnemyDirectionChanged));
    }

    private void OnVisionAreaBodyEntered(Node body)
    {
        if (body is Player)
            EmitSignal(SignalName.PlayerEntered);
    }

    private void OnVisionAreaBodyExited(Node body)
    {
        if (body is Player)
            EmitSignal(SignalName.PlayerExited);
    }

    private void OnEnemyDirectionChanged(Vector2 newDirection)
    {
        if (newDirection == Vector2.Down)
            RotationDegrees = 0;
        else if (newDirection == Vector2.Up)
            RotationDegrees = 180;
        else if (newDirection == Vector2.Left)
            RotationDegrees = 90;
        else
            RotationDegrees = -90;
    }
}
