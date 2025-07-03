using Godot;

public class ShopKeeper : Node2D
{
    // Exports
    [Export]
    private readonly Items[] shopInventory;

    // private
    private ShopMenu shopMenu;
    private readonly PackedScene shopMenuScene = GD.Load<PackedScene>("res://GUI/shopMenu/ShopMenu.tscn");

    // methods
    public override void _Ready()
    {
        DialogSystem.Instance.Connect(nameof(DialogSystem.BranchSelected), this, nameof(OnDialogBranchSelected));
    }

    private void OnDialogBranchSelected(int index)
    {
        if (index != 0)
            return;

        shopMenu = (ShopMenu)shopMenuScene.Instance();
        AddChild(shopMenu);
        shopMenu.PopulateItemList(shopInventory);
        shopMenu.SetMenu(true);
    }
}
