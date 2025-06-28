using Godot;

public class ShopItemButton : Button
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
        font = GetFont("m5x7");
    }

    public void SetupItem(Items items)
    {
        label.Text = items.Name;
        priceLabel.Text = items.Cost.ToString();
        textureRect.Texture = items.Texture;
        ellipsis.Visible = font.GetStringSize(label.Text).x > label.RectSize.x;
    }
}
