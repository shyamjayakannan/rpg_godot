using Godot;

[Tool]
public class AbilityButton : Button
{
    // Exports
    [Export(PropertyHint.MultilineText)]
    public string description;
    [Export]
    public Texture Texture
    {
        get => texture;
        set
        {
            texture = value;

            if (Engine.EditorHint)
                UpdateTexture();
        }
    }

    // private
    private Texture texture;
    private TextureRect textureRect;
    private Label label;

    // methods
    public override void _Ready()
    {
        textureRect = GetNode<TextureRect>("TextureRect");
        label = GetNode<Label>("Label");

        UpdateTexture();

        if (Engine.EditorHint)
            return;
    }

    private void UpdateTexture()
    {
        if (textureRect != null)
            textureRect.Texture = Texture;
    }

    public void UpdateLabel(int number)
    {
        if (label != null)
            label.Text = number.ToString();
    }
}
