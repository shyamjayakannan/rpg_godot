using Godot;

public class VisionArea : Area2D
{
    // Signals
    [Signal]
    public delegate void PlayerEntered();
    [Signal]
    public delegate void PlayerExited();

    // methods
    public override void _Ready()
    {
        Connect("body_entered", this, nameof(OnVisionAreaBodyEntered));
        Connect("body_exited", this, nameof(OnVisionAreaBodyExited));

        if (GetParent() is Enemy parent)
            parent.Connect(nameof(Enemy.EnemyDirectionChanged), this, nameof(OnEnemyDirectionChanged));
    }

    private void OnVisionAreaBodyEntered(Node body)
    {
        if (body is Player)
            EmitSignal(nameof(PlayerEntered));
    }

    private void OnVisionAreaBodyExited(Node body)
    {
        if (body is Player)
            EmitSignal(nameof(PlayerExited));
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
