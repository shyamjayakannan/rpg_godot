using Godot;

public partial class BarredDoor : Node2D
{
    // private
    private AnimationPlayer animationPlayer;
    private PersistentDataHandler persistentDataHandler;

    // methods
    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        persistentDataHandler = GetNode<PersistentDataHandler>("PersistentDataHandler");

        persistentDataHandler.Connect(PersistentDataHandler.SignalName.DataLoaded, new(this, MethodName.SetState));
        persistentDataHandler.GetValue();

        GlobalSignalManager.Instance.Connect(GlobalSignalManager.SignalName.PressurePlateActivated, new(this, MethodName.OpenDoor));
        GlobalSignalManager.Instance.Connect(GlobalSignalManager.SignalName.PressurePlateDeactivated, new(this, MethodName.CloseDoor));
    }

    private void SetState(bool value)
    {
        if (!value)
            return;

        animationPlayer.Play("opened");
    }

    private void OpenDoor()
    {
        animationPlayer.Play("openDoor");
        persistentDataHandler.SetValue();
        GlobalSignalManager.Instance.EmitSignal(GlobalSignalManager.SignalName.BarredDoorStateChanged, true);
    }

    private void CloseDoor()
    {
        animationPlayer.Play("closeDoor");
        persistentDataHandler.UnsetValue();
        GlobalSignalManager.Instance.EmitSignal(GlobalSignalManager.SignalName.BarredDoorStateChanged, false);
    }
}
