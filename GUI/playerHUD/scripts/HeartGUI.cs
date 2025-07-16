using Godot;

namespace Rpg
{
    public partial class HeartGUI : Control
    {
        // private
        private Sprite2D heartSprite;
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
            heartSprite = GetNode<Sprite2D>("Sprite2D");
        }

        private void UpdateSprite()
        {
            if (heartSprite != null)
                heartSprite.Frame = FrameNumber;
        }
    }
}
