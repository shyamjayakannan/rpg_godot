using Godot;

public class GlobalSignalManager : Node
{
    // Signals
    [Signal]
    public delegate void PressurePlateActivated();
    [Signal]
    public delegate void PressurePlateDeactivated();
    [Signal]
    public delegate void BarredDoorStateChanged(bool value);
    [Signal]
    public delegate void EnemiesDestroyed(bool alreadyDestroyed);

    // properties
    public static GlobalSignalManager Instance { get; private set; }

    // methods
    public override void _Ready()
    {
        Instance = this;
    }
}