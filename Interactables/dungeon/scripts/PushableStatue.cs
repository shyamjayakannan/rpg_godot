using Godot;

public class PushableStatue : RigidBody2D
{
    // Exports
    [Export]
    private readonly float pushSpeed = 60.0f;
    [Export]
    private readonly Vector2 targetPosition;
    [Export]
    private readonly bool usePersistence = false;

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

            persistentDataHandler.Connect(nameof(PersistentDataHandler.DataLoaded), this, nameof(SetState));
            persistentDataHandler.GetValue();
        }

        GlobalSignalManager.Instance.Connect(nameof(GlobalSignalManager.BarredDoorStateChanged), this, nameof(SetValue));
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
        if (value && targetPosition != null)
        {
            Position = targetPosition;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        Rotation = 0;
        LinearVelocity.LimitLength(pushSpeed);

        if (LinearVelocity.Length() < 0.1 && audioStreamPlayer2D.Playing)
            audioStreamPlayer2D.Stop();
        else if (!audioStreamPlayer2D.Playing)
            audioStreamPlayer2D.Play();
    }
}
