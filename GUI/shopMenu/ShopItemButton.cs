using Godot;

public partial class ShopItemButton : Button
{
    // private
    private Label label;
    private Label priceLabel;
    private Label ellipsis;
    private TextureRect textureRect;
    private Font font;

    // methods
    public override void _Ready()
    {
        label = GetNode<Label>("HBoxContainer/MarginContainer2/Label");
        priceLabel = GetNode<Label>("HBoxContainer/MarginContainer/Label2");
        ellipsis = GetNode<Label>("HBoxContainer/Ellipsis");
        textureRect = GetNode<TextureRect>("HBoxContainer/TextureRect");
        font = GetThemeFont("m5x7");
    }

    public void SetupItem(Items items)
    {
        label.Text = items.Name;
        priceLabel.Text = items.Cost.ToString();
        textureRect.Texture = items.Texture2D;
        ellipsis.Visible = font.GetStringSize(label.Text).X > label.Size.X;
    }
}
