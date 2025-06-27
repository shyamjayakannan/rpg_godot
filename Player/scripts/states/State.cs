using Godot;

public class State : Node
{
    // properties
    public static Player Player { get; set; }
    public static PlayerStateMachine StateMachine { get; set; }

    // methods
    public virtual void Init()
    {

    }

    public virtual void Enter()
    {

    }

    public virtual void Exit()
    {
    }

    public virtual State Process(float delta)
    {
        return null;
    }

    public virtual State PhysicsProcess(float delta)
    {
        return null;
    }

    public virtual State HandleInput(InputEvent _event)
    {
        return null;
    }
}
