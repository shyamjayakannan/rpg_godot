using Godot;

namespace Rpg
{
    public partial class Interactions : Node2D
    {
        // methods
        public override void _Ready()
        {
            GlobalPlayerManager.Instance.Player.Connect(Player.SignalName.PlayerDirectionChanged, new(this, MethodName.OnPlayerDirectionChanged));
        }

        private void OnPlayerDirectionChanged(Vector2 newDirection)
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
}
