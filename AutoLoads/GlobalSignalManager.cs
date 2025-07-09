using Godot;

public partial class GlobalSignalManager : Node
{
    // Signals
    [Signal]
    public delegate void PressurePlateActivatedEventHandler();
    [Signal]
    public delegate void PressurePlateDeactivatedEventHandler();
    [Signal]
    public delegate void BarredDoorStateChangedEventHandler(bool value);
    [Signal]
    public delegate void EnemiesDestroyedEventHandler(bool alreadyDestroyed);

    // properties
    public static GlobalSignalManager Instance { get; private set; }

    // methods
    public override void _Ready()
    {
        Instance = this;
    }
}