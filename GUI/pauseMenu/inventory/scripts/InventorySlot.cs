using Godot;

public class InventorySlot : Button
{
	// Signals
	[Signal]
	public delegate void EquipmentSelected(EquipableItem item);
	[Signal]
	public delegate void EquipmentFocused(EquipableItem item, bool update);
	[Signal]
	public delegate void MouseEntered();

	// private
	private SlotData slotData;
	private TextureRect textureRect;
	private Label label;
	private bool buttonDown = false;
	private TextureRect dragTexture;
	private readonly float dragThreshold = 0.2f;
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

			textureRect.Texture = slotData.Item.Texture;
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

		Connect("pressed", this, nameof(OnInventorySlotPressed));
		Connect("focus_entered", this, nameof(OnInventorySlotFocusEntered));
		Connect("focus_exited", this, nameof(OnInventorySlotFocusExited));
		Connect("button_down", this, nameof(OnButtonDown));
	}

	public override void _Process(float delta)
	{
		if (GetGlobalRect().HasPoint(GetGlobalMousePosition()))
		{
			if (!alreadyEmitted)
			{
				alreadyEmitted = true;
				EmitSignal(nameof(MouseEntered));
			}
		}
		else
			alreadyEmitted = false;

		if (buttonDown && !Dragging)
		{
			timer += delta;

			if (timer > dragThreshold)
			{
				timer = 0;
				Dragging = true;
			}
		}
		else
			timer = 0;

		if (Dragging)
			dragTexture.RectGlobalPosition = GetGlobalMousePosition() - dragTexture.RectSize / 2;
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
		dragTexture.RectGlobalPosition = textureRect.RectGlobalPosition;
		dragTexture.MouseFilter = MouseFilterEnum.Ignore;
		PauseMenu.Instance.AddChild(dragTexture);
	}

	private void PlayAudio(AudioStream audioStream)
	{
		if (AudioStreamPlayer.IsConnected("finished", this, nameof(PlayAudio)))
			AudioStreamPlayer.Disconnect("finished", this, nameof(PlayAudio));

		AudioStreamPlayer.Stream = audioStream;
		AudioStreamPlayer.Play();
	}

	private void OnInventorySlotFocusEntered()
	{
		if (AudioStreamPlayer.Playing)
			AudioStreamPlayer.Connect("finished", this, nameof(PlayAudio), new Godot.Collections.Array(ButtonMenu.ButtonFocusSound));
		else
			PlayAudio(ButtonMenu.ButtonFocusSound);

		if (slotData == null)
			return;

		if (slotData.Item is EquipableItem equipableItem)
		{
			EmitSignal(nameof(EquipmentFocused), equipableItem, false);
			PauseMenu.Instance.UpdateDescription(SlotData.Item.Description + "\n\n" + equipableItem.StatsDescription);
			return;
		}

		PauseMenu.Instance.UpdateDescription(SlotData.Item.Description);
	}

	private void OnInventorySlotFocusExited()
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
			EmitSignal(nameof(EquipmentSelected), equipableItem);
			return;
		}

		if (slotData.Quantity == 1)
		{
			QueueFree();
			int index = GetPositionInParent();
			GlobalPlayerManager.Instance.PlayerInventory.Slots.RemoveAt(index);
			PauseMenu.Instance.EmitSignal(nameof(PauseMenu.ItemRemoved));

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
