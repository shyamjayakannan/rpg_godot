using Godot;

public partial class GlobalPlayerManager : Node
{
    // Signals
    [Signal]
    public delegate void InteractPressedEventHandler();

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
        Player = (Player)GD.Load<PackedScene>("res://Player/Player.tscn").Instantiate();
        YSortHandler.YSortHandlerScene.Instantiate().AddChild(Player);

        // we dont have a way to know if playerspawn nodes exist and we want PlayerSpawned to be true
        // so we wait a bit so that a PlayerSpawn node, if it exists, can set it to true.
        // we cant set it to true in the beginninng because PlayerSpawn nodes require it to be false first.
        GetTree().CreateTimer(0.2f, false).Connect(SceneTreeTimer.SignalName.Timeout, Callable.From(() => PlayerSpawned = true));
    }

    public void SetPlayerPosition(Vector2 position)
    {
        PlayerSpawned = true;
        ((YSortHandler)Player.GetParent()).GlobalPosition = position;
    }

    public void SetParent(Node parent)
    {
        Node p = RemovePlayerParent();
        parent.AddChild(p);
    }

    public Node RemovePlayerParent()
    {
        Node parent = Player.GetParent();
        parent.GetParent()?.RemoveChild(parent);
        return parent;
    }

    public bool IsEquipmentPresent(EquipableItem equipableItem)
    {
        return PlayerInventory.IsEquipmentPresent(equipableItem) || PlayerEquipmentInventory.IsEquipmentPresent(equipableItem);
    }
}