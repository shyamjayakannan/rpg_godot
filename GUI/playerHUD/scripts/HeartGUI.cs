using Godot;

public class HeartGUI : Control
{
    // private
    private Sprite heartSprite;
    private int frameNumber;

    // properties
    public int FrameNumber
    {
        get => frameNumber;
        set
        {
            frameNumber = value;
            UpdateSprite();
        }
    }

    // methods
    public override void _Ready()
    {
        heartSprite = GetNode<Sprite>("Sprite");
    }

    private void UpdateSprite()
    {
        if (heartSprite != null)
            heartSprite.Frame = FrameNumber;
    }
}
