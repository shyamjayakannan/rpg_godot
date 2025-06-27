using Godot;

public class EnergyBeam : Node2D
{
    // private
    private AnimationPlayer animationPlayer;

    // methods
    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void Attack()
    {
        animationPlayer.Play("attack");
    }

    public void Stop()
    {
        animationPlayer.Stop();
    }
}
