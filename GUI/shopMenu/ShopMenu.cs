using Godot;

public class ShopMenu : CanvasLayer
{
    // Signals
    [Signal]
    private delegate void Shown();
    [Signal]
    private delegate void Hidden();

    // private
    private AcceptDialog acceptDialog;
    private ColorRect colorRect;
    private readonly PackedScene shopItemButtonScene = GD.Load<PackedScene>("res://GUI/shopMenu/ShopItemButton.tscn");
    private readonly Items gem = GD.Load<Items>("res://Items/items/gem.tres");
    private Items currentItem;
    private Button closeButton;
    private Button buyButton;
    private ButtonMenu buttonMenu;
    private AudioStreamPlayer audioStreamPlayer;
    private TextureRect textureRect;
    private Label title;
    private Label description;
    private Label price;
    private Label inInventory;
    private Label gems;
    private Label total;
    private LineEdit lineEdit;
    private AnimationPlayer animationPlayer;
    private readonly AudioStream openShopAudio = GD.Load<AudioStream>("res://GUI/shopMenu/audio/open_shop.wav");
    private readonly AudioStream purchaseAudio = GD.Load<AudioStream>("res://GUI/shopMenu/audio/purchase.wav");
    private readonly AudioStream errorAudio = GD.Load<AudioStream>("res://GUI/shopMenu/audio/error.wav");

    // methods
    public override void _Ready()
    {
        closeButton = GetNode<Button>("Control/Close");
        buyButton = GetNode<Button>("Control/Items/Control/Buy");
        audioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        buttonMenu = GetNode<ButtonMenu>("Control/ScrollContainer/ButtonMenu");
        textureRect = GetNode<TextureRect>("Control/Items/Control/TextureRect");
        title = GetNode<Label>("Control/Items/Control/VBoxContainer/Title");
        description = GetNode<Label>("Control/Items/Control/VBoxContainer/Description");
        price = GetNode<Label>("Control/Items/Control/Price");
        inInventory = GetNode<Label>("Control/Items/Control/InInventory");
        gems = GetNode<Label>("Control/HBoxContainer/Label");
        total = GetNode<Label>("Control/Items/Control/Total2");
        lineEdit = GetNode<LineEdit>("Control/Items/Control/Quantity/LineEdit");
        animationPlayer = GetNode<AnimationPlayer>("Control/HBoxContainer/AnimationPlayer");
        acceptDialog = GetNode<AcceptDialog>("AcceptDialog");
        colorRect = GetNode<ColorRect>("ColorRect2");
        colorRect.Hide();
        Initialize();

        acceptDialog.Connect("popup_hide", this, nameof(OnPopupHidden));
        buyButton.Connect("pressed", this, nameof(OnBuyButtonPressed));
        lineEdit.Connect("text_changed", this, nameof(OnLineEditTextChanged));
        closeButton.Connect("pressed", this, nameof(SetMenu), new Godot.Collections.Array(false));
    }

    private void Initialize()
    {
        price.Text = "";
        textureRect.Texture = null;
        title.Text = "";
        description.Text = "";
        lineEdit.Text = "1";
        total.Text = "";
    }

    private void OnPopupHidden()
    {
        colorRect.Hide();
    }

    private void OnBuyButtonPressed()
    {
        if (total.Text.ToInt() > gems.Text.ToInt())
        {
            animationPlayer.Play("notEnoughMoney");
            PlayAudio(errorAudio);
            return;
        }

        PlayAudio(purchaseAudio);
        int remaining = gems.Text.ToInt() - total.Text.ToInt();
        int added = lineEdit.Text.ToInt();
        gems.Text = remaining.ToString();
        GlobalPlayerManager.Instance.PlayerInventory.RemoveItem(gem, remaining);
        inInventory.Text = (inInventory.Text.ToInt() + added).ToString();
        GlobalPlayerManager.Instance.PlayerInventory.AddItem(currentItem, added);
    }

    private void OnLineEditTextChanged(string newText)
    {
        int cursorPos = lineEdit.CaretPosition;
        string filtered = "";

        foreach (char c in newText)
            if (char.IsDigit(c))
                filtered += c;

        lineEdit.Text = filtered.Length == 0 ? "0" : filtered;
        lineEdit.CaretPosition = Mathf.Min(cursorPos, lineEdit.Text.Length);

        total.Text = (lineEdit.Text.ToInt() * currentItem.Cost).ToString();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible || !@event.IsActionPressed("ui_cancel"))
            return;

        // dont let pausemenu be called when closing using esc key
        GetTree().SetInputAsHandled();
        SetMenu(false);
    }

    public void SetMenu(bool value)
    {
        EmitSignal(value ? nameof(Shown) : nameof(Hidden));
        GlobalPlayerManager.Instance.Player.SetProcessUnhandledInput(!value);
        GlobalPlayerManager.Instance.Player.ChangeStateToIdle();

        if (value)
        {
            Visible = value;
            PlayAudio(openShopAudio);
            gems.Text = GlobalPlayerManager.Instance.PlayerInventory.GetQuantity(gem).ToString();
        }
        else
            QueueFree();
    }

    private void PlayAudio(AudioStream audioStream)
    {
        audioStreamPlayer.Stream = audioStream;
        audioStreamPlayer.Play();
    }

    public void PopulateItemList(Items[] items)
    {
        foreach (Node child in buttonMenu.GetChildren())
            child.QueueFree();

        foreach (Items item in items)
        {
            ShopItemButton shopItemButton = (ShopItemButton)shopItemButtonScene.Instance();
            buttonMenu.AddChild(shopItemButton);
            shopItemButton.SetupItem(item);
            buttonMenu.ConnectFocus(shopItemButton, audioStreamPlayer);
            shopItemButton.Connect("focus_entered", this, nameof(OnShopItemButtonFocused), new Godot.Collections.Array(item));
        }

        GetTree().CreateTimer(0.1f).Connect("timeout", this, nameof(OnTimerTimeout));
    }

    private void OnTimerTimeout()
    {
        buttonMenu.GetChildOrNull<ShopItemButton>(0)?.GrabFocus();
    }

    private void OnShopItemButtonFocused(Items item)
    {
        currentItem = item;
        price.Text = item.Cost.ToString();
        textureRect.Texture = item.Texture;
        title.Text = item.Name;
        description.Text = item.Description;
        lineEdit.Text = "1";
        total.Text = price.Text;
        int quantity = GlobalPlayerManager.Instance.PlayerInventory.GetQuantity(item);

        if (item is EquipableItem)
        {
            if (quantity == 0 && GlobalPlayerManager.Instance.PlayerEquipmentInventory.GetQuantity(item) == 0)
            {
                inInventory.Text = "0";
                return;
            }

            colorRect.Show();
            acceptDialog.Popup_();
        }

        inInventory.Text = quantity.ToString();
    }
}
