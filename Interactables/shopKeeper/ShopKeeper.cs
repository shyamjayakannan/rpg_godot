using Godot;

public partial class ShopKeeper : Node2D
{
    // Exports
    [Export]
    private Items[] shopInventory;

    // private
    private ShopMenu shopMenu;
    private PackedScene shopMenuScene = GD.Load<PackedScene>("res://GUI/shopMenu/ShopMenu.tscn");

    // methods
    public override void _Ready()
    {
        DialogSystem.Instance.Connect(DialogSystem.SignalName.BranchSelected, new(this, MethodName.OnDialogBranchSelected));
    }

    private void OnDialogBranchSelected(int index)
    {
        if (index != 0)
            return;

        shopMenu = (ShopMenu)shopMenuScene.Instantiate();
        AddChild(shopMenu);
        shopMenu.PopulateItemList(shopInventory);
        shopMenu.SetMenu(true);
    }
}
