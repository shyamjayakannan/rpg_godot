using Godot;

public partial class InventorySlot : Button
{
	// Signals
	[Signal]
	public delegate void EquipmentSelectedEventHandler(EquipableItem item);
	[Signal]
	public delegate void EquipmentFocusedEventHandler(EquipableItem item, bool update);
	[Signal]
	public delegate void MouseEnteredEventHandler();

	// private
	private SlotData slotData;
	private TextureRect textureRect;
	private Label label;
	private bool buttonDown = false;
	private TextureRect dragTexture;
	private float dragThreshold = 0.2f;
	private float timer = 0;
	private bool alreadyEmitted = false;

	// properites
	public bool Dragging = false;
	public SlotData SlotData
	{
		get => slotData;
		set
		{
			slotData = value;

			if (value == null)
			{
				textureRect.Texture = null;
				label.Text = "";
				return;
			}

			textureRect.Texture = slotData.Item.Texture2D;
			label.Text = slotData.Item is EquipableItem ? "" : slotData.Quantity.ToString();
		}
	}
	public static AudioStreamPlayer AudioStreamPlayer { get; set; }

	// methods
	public override void _Ready()
	{
		textureRect = GetNode<TextureRect>("TextureRect");
		label = GetNode<Label>("Label");

		textureRect.Texture = null;
		label.Text = "";

		Connect(BaseButton.SignalName.Pressed, new(this, MethodName.OnInventorySlotPressed));
		Connect(Control.SignalName.FocusEntered, new(this, MethodName.OnInventorySlotFocusEntered));
		Connect(Control.SignalName.FocusExited, new(this, MethodName.OnInventorySlotFocusExited));
		Connect(BaseButton.SignalName.ButtonDown, new(this, MethodName.OnButtonDown));
	}

	public override void _Process(double delta)
	{
		if (GetGlobalRect().HasPoint(GetGlobalMousePosition()))
		{
			if (!alreadyEmitted)
			{
				alreadyEmitted = true;
				EmitSignal(SignalName.MouseEntered);
			}
		}
		else
			alreadyEmitted = false;

		if (buttonDown && !Dragging)
		{
			timer += (float)delta;

			if (timer > dragThreshold)
			{
				timer = 0;
				Dragging = true;
			}
		}
		else
			timer = 0;

		if (Dragging)
			dragTexture.GlobalPosition = GetGlobalMousePosition() - dragTexture.Size / 2;
	}

	public void OnButtonUp()
	{
		Dragging = false;
		buttonDown = false;
		dragTexture?.QueueFree();
	}

	private void OnButtonDown()
	{
		buttonDown = true;
		dragTexture = (TextureRect)textureRect.Duplicate();
		dragTexture.GlobalPosition = textureRect.GlobalPosition;
		dragTexture.MouseFilter = MouseFilterEnum.Ignore;
		PauseMenu.Instance.AddChild(dragTexture);
	}

	private void PlayAudio(AudioStream audioStream)
	{
		if (AudioStreamPlayer.IsConnected(AudioStreamPlayer.SignalName.Finished, new(this, MethodName.PlayAudio)))
			AudioStreamPlayer.Disconnect(AudioStreamPlayer.SignalName.Finished, new(this, MethodName.PlayAudio));

		AudioStreamPlayer.Stream = audioStream;
		AudioStreamPlayer.Play();
	}

	private void OnInventorySlotFocusEntered()
	{
		if (AudioStreamPlayer.Playing)
			AudioStreamPlayer.Connect(AudioStreamPlayer.SignalName.Finished, Callable.From(() => PlayAudio(ButtonMenu.ButtonFocusSound)));
		else
			PlayAudio(ButtonMenu.ButtonFocusSound);

		if (slotData == null)
			return;

		if (slotData.Item is EquipableItem equipableItem)
		{
			EmitSignal(SignalName.EquipmentFocused, equipableItem, false);
			PauseMenu.Instance.UpdateDescription(SlotData.Item.Description + "\n\n" + equipableItem.StatsDescription);
			return;
		}

		PauseMenu.Instance.UpdateDescription(SlotData.Item.Description);
	}

	private static void OnInventorySlotFocusExited()
	{
		PauseMenu.Instance.UpdateDescription("");
		PauseMenu.Instance.Stats.UpdateStats(0, 0);
	}

	private void OnInventorySlotPressed()
	{
		if (slotData == null || Dragging)
			return;

		PlayAudio(ButtonMenu.ButtonPressSound);
		slotData.Item.Use();

		if (slotData.Item is EquipableItem equipableItem)
		{
			EmitSignal(SignalName.EquipmentSelected, equipableItem);
			return;
		}

		if (slotData.Quantity == 1)
		{
			QueueFree();
			int index = GetIndex();
			GlobalPlayerManager.Instance.PlayerInventory.Slots.RemoveAt(index);
			PauseMenu.Instance.EmitSignal(PauseMenu.SignalName.ItemRemoved);

			// focus on the first item
			if (index == 0)
				GetParent().GetChildOrNull<Button>(1)?.GrabFocus();
			else
				GetParent().GetChild<Button>(0).GrabFocus();
		}
		else
		{
			slotData.Quantity--;
			label.Text = slotData.Quantity.ToString();
		}
	}
}
