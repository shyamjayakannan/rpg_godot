using Godot;

public class EquipmentUI : HBoxContainer
{
    // Exports
    [Export]
    private readonly InventoryData equipment;

    // private
    private readonly PackedScene inventorySlotScene = GD.Load<PackedScene>("res://GUI/pauseMenu/inventory/InventorySlot.tscn");
    private HBoxContainer[] equipmentContainers;

    // methods
    public override void _Ready()
    {
        base._Ready();
    }
}
