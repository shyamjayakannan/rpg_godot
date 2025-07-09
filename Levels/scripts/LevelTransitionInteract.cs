using Godot;

[Tool]
public partial class LevelTransitionInteract : LevelTransition
{
    // methods
    public override void _Ready()
    {
        base._Ready();
        Disconnect(Area2D.SignalName.BodyEntered, new(this, LevelTransition.MethodName.OnLevelTransitionBodyEntered));
        Connect(Area2D.SignalName.AreaEntered, new(this, MethodName.OnArea2DAreaEntered));
        Connect(Area2D.SignalName.AreaExited, new(this, MethodName.OnArea2DAreaExited));
    }

    private void OnArea2DAreaEntered(Area2D area)
    {
        GlobalPlayerManager.Instance.Connect(GlobalPlayerManager.SignalName.InteractPressed, new(this, LevelTransition.MethodName.OnLevelTransitionBodyEntered));
    }

    private void OnArea2DAreaExited(Area2D area)
    {
        GlobalPlayerManager.Instance.Disconnect(GlobalPlayerManager.SignalName.InteractPressed, new(this, LevelTransition.MethodName.OnLevelTransitionBodyEntered));
    }
}
