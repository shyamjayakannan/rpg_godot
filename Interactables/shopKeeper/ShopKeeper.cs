using Godot;

public class ShopKeeper : Node2D
{
    // Exports
    [Export]
    private readonly Items[] shopInventory;

    // private
    private DialogBranch dialogBranch;
    private ShopMenu shopMenu;
    private readonly PackedScene shopMenuScene = GD.Load<PackedScene>("res://GUI/shopMenu/ShopMenu.tscn");

    // methods
    public override void _Ready()
    {
        dialogBranch = GetNode<DialogBranch>("Npc/DialogInteraction/DialogChoice/DialogBranch");

        dialogBranch.Connect(nameof(DialogBranch.Selected), this, nameof(OnDialogBranchSelected));
    }

    private void OnDialogBranchSelected()
    {
        shopMenu = (ShopMenu)shopMenuScene.Instance();
        AddChild(shopMenu);
        shopMenu.SetMenu(true);
    }
}
