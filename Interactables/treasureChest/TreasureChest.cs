using Godot;

namespace Rpg
{
    [Tool]
    public partial class TreasureChest : Interactables
    {
        // Exports
        [Export]
        private Items Item
        {
            get => item;
            set
            {
                item = value;

                if (Engine.IsEditorHint())
                    UpdateTexture();
            }
        }
        [Export]
        private int Quantity
        {
            get => quantity;
            set
            {
                quantity = value;

                if (Engine.IsEditorHint())
                    UpdateQuantity();
            }
        }

        // private
        private Items item;
        private int quantity = 1;
        private Sprite2D sprite;
        private Label label;
        private AnimationPlayer animationPlayer;
        private bool isOpened = false;
        private Area2D area2D;
        private PersistentDataHandler persistentDataHandler;

        // methods
        public override void _Ready()
        {
            sprite = GetNode<Sprite2D>("ItemSprite");
            label = GetNode<Label>("ItemSprite/Label");
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            area2D = GetNode<Area2D>("Area2D");

            UpdateTexture();
            UpdateQuantity();

            if (Engine.IsEditorHint())
                return;

            // VERY IMPORTANT
            // since persistentdata handler isnt a tool script, it wont be recognized in the editor and we anyway dont need it
            // when code runs in editor so thats why declaring it here. otherwise, we have to make it a tool script too.
            persistentDataHandler = GetNode<PersistentDataHandler>("PersistentDataHandler");

            area2D.Connect(Area2D.SignalName.AreaEntered, new(this, Interactables.MethodName.OnArea2DAreaEntered));
            area2D.Connect(Area2D.SignalName.AreaExited, new(this, Interactables.MethodName.OnArea2DAreaExited));

            persistentDataHandler.Connect(PersistentDataHandler.SignalName.DataLoaded, new(this, MethodName.SetChestState));
            persistentDataHandler.GetValue();
        }

        private void UpdateTexture()
        {
            if (sprite != null)
                sprite.Texture = Item?.Texture2D;
        }

        private void UpdateQuantity()
        {
            if (label != null)
                label.Text = $"x{Quantity}";
        }

        public override void OnInteractPressed()
        {
            if (isOpened)
                return;

            isOpened = true;
            animationPlayer.Play("openChest");
            GlobalPlayerManager.Instance.PlayerInventory.AddItem(Item, Quantity);
            persistentDataHandler.SetValue();
        }

        private void SetChestState(bool value)
        {
            isOpened = value;

            if (isOpened)
                animationPlayer.Play("opened");
            else
                animationPlayer.Play("closed");
        }
    }
}
