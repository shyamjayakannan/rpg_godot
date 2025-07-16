using Godot;

namespace Rpg
{
    public partial class ShopMenu : CanvasLayer
    {
        // Signals
        [Signal]
        private delegate void ShownEventHandler();
        [Signal]
        private delegate void HiddenEventHandler();

        // private
        private AcceptDialog acceptDialog;
        private ColorRect colorRect;
        private PackedScene shopItemButtonScene = GD.Load<PackedScene>("res://GUI/shopMenu/ShopItemButton.tscn");
        private Items gem = GD.Load<Items>("res://Items/items/gem.tres");
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
        private SpinBox spinBox;
        private AnimationPlayer animationPlayer;
        private AudioStream openShopAudio = GD.Load<AudioStream>("res://GUI/shopMenu/audio/open_shop.wav");
        private AudioStream purchaseAudio = GD.Load<AudioStream>("res://GUI/shopMenu/audio/purchase.wav");
        private AudioStream errorAudio = GD.Load<AudioStream>("res://GUI/shopMenu/audio/error.wav");

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
            spinBox = GetNode<SpinBox>("Control/Items/Control/Quantity/SpinBox");
            animationPlayer = GetNode<AnimationPlayer>("Control/HBoxContainer/AnimationPlayer");
            acceptDialog = GetNode<AcceptDialog>("AcceptDialog");
            colorRect = GetNode<ColorRect>("ColorRect2");
            colorRect.Hide();
            Initialize();

            acceptDialog.Connect(AcceptDialog.SignalName.Canceled, Callable.From(() => colorRect.Hide()));
            buyButton.Connect(BaseButton.SignalName.Pressed, new(this, MethodName.OnBuyButtonPressed));
            spinBox.Connect(Range.SignalName.ValueChanged, new(this, MethodName.OnSpinBoxValueChanged));
            closeButton.Connect(BaseButton.SignalName.Pressed, Callable.From(() => SetMenu(false)));
        }

        private void Initialize()
        {
            price.Text = "";
            textureRect.Texture = null;
            title.Text = "";
            description.Text = "";
            spinBox.Value = 1;
            total.Text = "";
        }

        private void OnBuyButtonPressed()
        {
            if (total.Text.ToInt() > gems.Text.ToInt())
            {
                animationPlayer.Play("notEnoughMoney");
                PlayAudio(errorAudio);
                return;
            }

            int added = (int)spinBox.Value;

            if (!GlobalPlayerManager.Instance.PlayerInventory.AddItem(currentItem, added))
            {
                if (currentItem is EquipableItem equipableItem && GlobalPlayerManager.Instance.IsEquipmentPresent(equipableItem))
                {
                    acceptDialog.DialogText = "The equipment is already present in the inventory";
                    acceptDialog.Title = "Equipment Duplicate Found!";
                }
                else
                {
                    acceptDialog.DialogText = "The Inventory is full";
                    acceptDialog.Title = "Alert!";
                }

                acceptDialog.Popup();
                colorRect.Show();
                PlayAudio(errorAudio);
                return;
            }

            PlayAudio(purchaseAudio);
            int totalCost = total.Text.ToInt();
            gems.Text = (gems.Text.ToInt() - totalCost).ToString();
            GlobalPlayerManager.Instance.PlayerInventory.RemoveItem(gem, totalCost);
            inInventory.Text = (inInventory.Text.ToInt() + added).ToString();
        }

        private void OnSpinBoxValueChanged(float value)
        {
            total.Text = (value * currentItem.Cost).ToString();
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (!Visible || !@event.IsActionPressed("ui_cancel"))
                return;

            // dont let pausemenu be called when closing using esc key
            GetViewport().SetInputAsHandled();
            SetMenu(false);
        }

        public void SetMenu(bool value)
        {
            EmitSignal(value ? SignalName.Shown : SignalName.Hidden);
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
                ShopItemButton shopItemButton = (ShopItemButton)shopItemButtonScene.Instantiate();
                buttonMenu.AddChild(shopItemButton);
                shopItemButton.SetupItem(item);
                buttonMenu.ConnectFocus(shopItemButton, audioStreamPlayer);
                shopItemButton.Connect(Control.SignalName.FocusEntered, Callable.From(() => OnShopItemButtonFocused(item)));
            }

            GetTree().CreateTimer(0.1f).Connect(SceneTreeTimer.SignalName.Timeout, Callable.From(() => buttonMenu.GetChildOrNull<ShopItemButton>(0)?.GrabFocus()));
        }

        private void OnShopItemButtonFocused(Items item)
        {
            currentItem = item;
            price.Text = item.Cost.ToString();
            textureRect.Texture = item.Texture2D;
            title.Text = item.Name;
            description.Text = item.Description;
            spinBox.Value = 1;
            total.Text = price.Text;
            int quantity = GlobalPlayerManager.Instance.PlayerInventory.GetQuantity(item);

            if (quantity == 0 && item is EquipableItem)
                quantity = GlobalPlayerManager.Instance.PlayerEquipmentInventory.GetQuantity(item);

            inInventory.Text = quantity.ToString();
        }
    }
}
