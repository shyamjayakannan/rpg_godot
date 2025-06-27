using Godot;

[Tool]
/// MAKE SURE ITEMDROPPER IS ABOVE ENEMYCOUNTER IN THE SCENETREE BECAUSE ITS ONREADY NEEDS TO BE CALLED FIRST.
/// THIS IS BECAUSE ITEMDROPPER'S ONREADY CONNECTS TO THE SIGNAL THAT IS FIRED BY THE GETVALUE CALL IN
/// ENEMYCOUNTER'S ONREADY. SIGNAL NEEDS TO BE CONNECTED BEFORE FIRING OTHERWISE WONT CATCH
public class ItemDropper : Node2D
{
    // Signals
    [Signal]
    private delegate void ItemPickedUp();

    // Exports
    [Export]
    public Items Item
    {
        get => item;
        set
        {
            item = value;

            if (Engine.EditorHint)
                UpdateTexture();
        }
    }

    // private
    private Items item;
    private Sprite sprite;
    private AudioStreamPlayer audioStreamPlayer;
    private readonly PackedScene itemPickupScene = GD.Load<PackedScene>("res://Items/itemPickup/ItemPickup.tscn");
    private bool hasDropped = false;
    private PersistentDataHandler persistentDataHandler;

    // methods
    public override void _Ready()
    {
        sprite = GetNode<Sprite>("Sprite");
        audioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        persistentDataHandler = GetNode<PersistentDataHandler>("PersistentDataHandler");

        if (Engine.EditorHint)
        {
            UpdateTexture();
            return;
        }

        sprite.Hide();
        GlobalSignalManager.Instance.Connect(nameof(GlobalSignalManager.EnemiesDestroyed), this, nameof(OnEnemiesDestroyed));

        persistentDataHandler.Connect(nameof(PersistentDataHandler.DataLoaded), this, nameof(SetHasDropped));
        persistentDataHandler.GetValue();
    }

    private void UpdateTexture()
    {
        if (sprite != null)
            sprite.Texture = Item.Texture;
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

        ItemPickup itemPickup = (ItemPickup)itemPickupScene.Instance();
        itemPickup.Item = item;
        itemPickup.Connect(nameof(ItemPickup.PickedUp), this, nameof(OnPickedUp));
        AddChild(itemPickup);
    }

    private void OnPickedUp()
    {
        persistentDataHandler.SetValue();
        EmitSignal(nameof(ItemPickedUp));
    }
}
