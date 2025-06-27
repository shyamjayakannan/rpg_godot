using Godot;

[Tool]
public class LevelTransitionInteract : LevelTransition
{
    // methods
    public override void _Ready()
    {
        base._Ready();
        Disconnect("body_entered", this, nameof(OnLevelTransitionBodyEntered));
        Connect("area_entered", this, nameof(OnArea2DAreaEntered));
        Connect("area_exited", this, nameof(OnArea2DAreaExited));
    }

    private void OnInteractPressed()
    {
        OnLevelTransitionBodyEntered();
    }

    private void OnArea2DAreaEntered(Area2D area)
    {
        GlobalPlayerManager.Instance.Connect(nameof(GlobalPlayerManager.InteractPressed), this, nameof(OnInteractPressed));
    }

    private void OnArea2DAreaExited(Area2D area)
    {
        GlobalPlayerManager.Instance.Disconnect(nameof(GlobalPlayerManager.InteractPressed), this, nameof(OnInteractPressed));
    }
}
