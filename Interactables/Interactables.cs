using Godot;

public abstract partial class Interactables : Node2D
{
    // methods
    public abstract void OnInteractPressed();

    protected virtual void OnArea2DAreaEntered(Area2D area = null)
    {
        GlobalPlayerManager.Instance.Connect(GlobalPlayerManager.SignalName.InteractPressed, new(this, MethodName.OnInteractPressed));
    }

    protected virtual void OnArea2DAreaExited(Area2D area = null)
    {
        GlobalPlayerManager.Instance.Disconnect(GlobalPlayerManager.SignalName.InteractPressed, new(this, MethodName.OnInteractPressed));
    }
}