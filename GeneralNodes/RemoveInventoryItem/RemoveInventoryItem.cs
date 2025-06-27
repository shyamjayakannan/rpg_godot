using Godot;

[Tool]
public class RemoveInventoryItem : Node
{
    // Exports
    [Export]
    private Items Item
    {
        get => item;
        set
        {
            item = value;
            UpdateConfigurationWarning();
        }
    }
    [Export]
    private int Quantity
    {
        get => quantity;
        set
        {
            quantity = value;
            UpdateConfigurationWarning();
        }
    }

    // private
    private Items item;
    private int quantity;

    // methods
    public void RemoveItemFromInventory()
    {
        GlobalPlayerManager.Instance.PlayerInventory.RemoveItem(item, quantity);
    }

    public override string _GetConfigurationWarning()
    {
        return Item == null || Quantity < 1 ? "please add an item with quantity at least 1" : "";
    }
}
