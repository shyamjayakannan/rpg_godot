using Godot;

public partial class PushableStatue : RigidBody2D
{
    // Exports
    [Export]
    private float pushSpeed = 60.0f;
    [Export]
    private Vector2 targetPosition;
    [Export]
    private bool usePersistence = false;

    // private
    private AudioStreamPlayer2D audioStreamPlayer2D;
    private PersistentDataHandler persistentDataHandler;

    // methods
    public override void _Ready()
    {
        audioStreamPlayer2D = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");

        if (usePersistence)
        {
            persistentDataHandler = GetNode<PersistentDataHandler>("PersistentDataHandler");

            persistentDataHandler.Connect(PersistentDataHandler.SignalName.DataLoaded, new(this, MethodName.SetState));
            persistentDataHandler.GetValue();
        }

        GlobalSignalManager.Instance.Connect(GlobalSignalManager.SignalName.BarredDoorStateChanged, new(this, MethodName.SetValue));
    }

    private void SetValue(bool value)
    {
        if (value)
            persistentDataHandler.SetValue();
        else
            persistentDataHandler.UnsetValue();
    }

    private void SetState(bool value)
    {
        if (value)
            Position = targetPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        Rotation = 0;
        LinearVelocity.LimitLength(pushSpeed);

        if (LinearVelocity.Length() < 0.1 && audioStreamPlayer2D.Playing)
            audioStreamPlayer2D.Stop();
        else if (!audioStreamPlayer2D.Playing)
            audioStreamPlayer2D.Play();
    }
}
