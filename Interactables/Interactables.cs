using Godot;

public abstract class Interactables : Node2D
{
    // methods
    public abstract void OnInteractPressed();

    protected virtual void OnArea2DAreaEntered(Area2D area = null)
    {
        GlobalPlayerManager.Instance.Connect(nameof(GlobalPlayerManager.InteractPressed), this, nameof(OnInteractPressed));
    }

    protected virtual void OnArea2DAreaExited(Area2D area = null)
    {
        GlobalPlayerManager.Instance.Disconnect(nameof(GlobalPlayerManager.InteractPressed), this, nameof(OnInteractPressed));
    }
}