using Godot;

namespace Rpg
{
    public partial class BowState : State
    {
        // private
        private State idleState;
        private State nextState = null;
        private PackedScene arrowScene = GD.Load<PackedScene>("res://Player/scripts/abilities/arrow/Arrow.tscn");

        // methods
        public override void _Ready()
        {
            idleState = GetNode<IdleState>("../IdleState");
        }

        public override void Enter()
        {
            Player.UpdateAnimation("bow");
            GetTree().CreateTimer(Player.AnimationPlayer.CurrentAnimationLength).Connect(SceneTreeTimer.SignalName.Timeout, Callable.From(() => nextState = idleState));

            Arrow arrow = (Arrow)arrowScene.Instantiate();
            YSortHandler.AddToScene(arrow, Player);
            arrow.GlobalPosition = Player.GlobalPosition + Player.CardinalDirection * 32;
            arrow.Fire(Player.CardinalDirection);
        }

        public override State Process(float delta)
        {
            Player.Velocity = Vector2.Zero;
            return nextState;
        }

        public override void Exit()
        {
            nextState = null;
        }
    }
}
