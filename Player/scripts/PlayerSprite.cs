using Godot;

public partial class PlayerSprite : Sprite2D
{
    // private
    private const int FRAME_COUNT = 128;
    private Sprite2D below;
    private Sprite2D above;

    // methods
    public override void _Ready()
    {
        below = GetNode<Sprite2D>("WeaponBelowSprite");
        above = GetNode<Sprite2D>("WeaponAboveSprite");

        PauseMenu.Instance.Connect(PauseMenu.SignalName.EquipmentsChanged, new(this, MethodName.OnEquipmentChanged));
    }

    private void OnEquipmentChanged(EquipableItem equipableItem)
    {
        if (equipableItem == null)
            // continue;
            return;

        if (equipableItem.EquipmentType == EquipableItem.Type.Armor)
        {

            Texture = equipableItem.SpriteTexture;
        }
        else
        {
            above.Texture = equipableItem.SpriteTexture;
            below.Texture = equipableItem.SpriteTexture;
        }
    }

    public override void _Process(double delta)
    {
        below.Frame = Frame;
        above.Frame = Frame + FRAME_COUNT;
    }
}
