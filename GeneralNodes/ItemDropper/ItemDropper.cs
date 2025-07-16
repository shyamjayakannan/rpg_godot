using Godot;

/// MAKE SURE ITEMDROPPER IS ABOVE ENEMYCOUNTER IN THE SCENETREE BECAUSE ITS ONREADY NEEDS TO BE CALLED FIRST.
/// THIS IS BECAUSE ITEMDROPPER'S ONREADY CONNECTS TO THE SIGNAL THAT IS FIRED BY THE GETVALUE CALL IN
/// ENEMYCOUNTER'S ONREADY. SIGNAL NEEDS TO BE CONNECTED BEFORE FIRING OTHERWISE WONT CATCH
namespace Rpg
{
    [Tool]
    public partial class ItemDropper : Node2D
    {
        // Signals
        [Signal]
        private delegate void ItemPickedUpEventHandler();

        // Exports
        [Export]
        public Items Item
        {
            get => item;
            set
            {
                item = value;

                if (Engine.IsEditorHint())
                    UpdateTexture();
            }
        }

        // private
        private Items item;
        private Sprite2D sprite;
        private AudioStreamPlayer audioStreamPlayer;
        private PackedScene itemPickupScene = GD.Load<PackedScene>("res://Items/itemPickup/ItemPickup.tscn");
        private bool hasDropped = false;
        private PersistentDataHandler persistentDataHandler;

        // methods
        public override void _Ready()
        {
            sprite = GetNode<Sprite2D>("Sprite2D");
            audioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
            persistentDataHandler = GetNode<PersistentDataHandler>("PersistentDataHandler");

            if (Engine.IsEditorHint())
            {
                UpdateTexture();
                return;
            }

            sprite.Hide();
            GlobalSignalManager.Instance.Connect(GlobalSignalManager.SignalName.EnemiesDestroyed, new(this, MethodName.OnEnemiesDestroyed));

            persistentDataHandler.Connect(PersistentDataHandler.SignalName.DataLoaded, new(this, MethodName.SetHasDropped));
            persistentDataHandler.GetValue();
        }

        private void UpdateTexture()
        {
            if (sprite != null)
                sprite.Texture = Item.Texture2D;
        }

        private void SetHasDropped(bool value)
        {
            hasDropped = value;
        }

        private void OnEnemiesDestroyed(bool alreadyDestroyed)
        {
            if (hasDropped)
                return;

            if (!alreadyDestroyed)
                audioStreamPlayer.Play();

            ItemPickup itemPickup = (ItemPickup)itemPickupScene.Instantiate();
            itemPickup.Item = item;
            itemPickup.Connect(ItemPickup.SignalName.PickedUp, new(this, MethodName.OnPickedUp));
            AddChild(itemPickup);
        }

        private void OnPickedUp()
        {
            persistentDataHandler.SetValue();
            EmitSignal(SignalName.ItemPickedUp);
        }
    }
}
