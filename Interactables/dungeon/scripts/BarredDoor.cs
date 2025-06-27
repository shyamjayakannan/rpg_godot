using System;
using Godot;

public class BarredDoor : Node2D
{
    // private
    private AnimationPlayer animationPlayer;
    private PersistentDataHandler persistentDataHandler;

    // methods
    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        persistentDataHandler = GetNode<PersistentDataHandler>("PersistentDataHandler");

        persistentDataHandler.Connect(nameof(PersistentDataHandler.DataLoaded), this, nameof(SetState));
        persistentDataHandler.GetValue();

        GlobalSignalManager.Instance.Connect(nameof(GlobalSignalManager.PressurePlateActivated), this, nameof(OpenDoor));
        GlobalSignalManager.Instance.Connect(nameof(GlobalSignalManager.PressurePlateDeactivated), this, nameof(CloseDoor));
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
        GlobalSignalManager.Instance.EmitSignal(nameof(GlobalSignalManager.BarredDoorStateChanged), true);
    }

    private void CloseDoor()
    {
        animationPlayer.Play("closeDoor");
        persistentDataHandler.UnsetValue();
        GlobalSignalManager.Instance.EmitSignal(nameof(GlobalSignalManager.BarredDoorStateChanged), false);
    }
}
