using Godot;

public class GlobalPlayerManager : Node
{
    // Signals
    [Signal]
    public delegate void InteractPressed();

    // properties
    public static GlobalPlayerManager Instance { get; private set; }
    public Player Player { get; set; }
    public bool PlayerSpawned { get; set; } = false;
    public InventoryData PlayerInventory { get; set; }
    public InventoryData PlayerEquipmentInventory { get; set; }

    // methods
    public override void _Ready()
    {
        Instance = this;
        Player = (Player)GD.Load<PackedScene>("res://Player/Player.tscn").Instance();

        // we dont have a way to know if playerspawn nodes exist and we want PlayerSpawned to be true
        // so we wait a bit so that a PlayerSpawn node, if it exists, can set it to true.
        // we cant set it to true in the beginninng because PlayerSpawn nodes require it to be false first.
        GetTree().CreateTimer(0.2f, false).Connect("timeout", this, nameof(Ready2));
    }

    private void Ready2()
    {
        PlayerSpawned = true;
    }

    public void SetPlayerPosition(Vector2 position)
    {
        PlayerSpawned = true;
        Player.GlobalPosition = position;
    }

    public void SetParent(Node parent)
    {
        RemovePlayerParent();
        parent.AddChild(Player);
    }

    public void RemovePlayerParent()
    {
        Player.GetParent()?.RemoveChild(Player);
    }
}