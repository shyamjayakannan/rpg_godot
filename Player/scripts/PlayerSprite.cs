using Godot;

public class PlayerSprite : Sprite
{
    // private
    private const int FRAME_COUNT = 128;
    private Sprite below;
    private Sprite above;

    // methods
    public override void _Ready()
    {
        below = GetNode<Sprite>("WeaponBelowSprite");
        above = GetNode<Sprite>("WeaponAboveSprite");

        PauseMenu.Instance.Connect(nameof(PauseMenu.EquipmentChanged), this, nameof(OnEquipmentChanged));
    }

    private void OnEquipmentChanged(EquipableItem equipableItem)
    {
        if (equipableItem.EquipmentType == EquipableItem.Type.Armor)
            Texture = equipableItem.SpriteTexture;
        else
        {
            above.Texture = equipableItem.SpriteTexture;
            below.Texture = equipableItem.SpriteTexture;
        }
    }

    public override void _Process(float delta)
    {
        below.Frame = Frame;
        above.Frame = Frame + FRAME_COUNT;
    }
}
