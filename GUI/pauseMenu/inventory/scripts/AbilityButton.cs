using Godot;

[Tool]
public partial class AbilityButton : Button
{
    // Exports
    [Export(PropertyHint.MultilineText)]
    public string description;
    [Export]
    public Texture2D Texture2D
    {
        get => texture;
        set
        {
            texture = value;

            if (Engine.IsEditorHint())
                UpdateTexture();
        }
    }

    // private
    private Texture2D texture;
    private TextureRect textureRect;
    private Label label;

    // methods
    public override void _Ready()
    {
        textureRect = GetNode<TextureRect>("TextureRect");
        label = GetNode<Label>("Label");

        UpdateTexture();

        if (Engine.IsEditorHint())
            return;
    }

    private void UpdateTexture()
    {
        if (textureRect != null)
            textureRect.Texture = Texture2D;
    }

    public void UpdateLabel(int number)
    {
        if (label != null)
            label.Text = number.ToString();
    }
}
