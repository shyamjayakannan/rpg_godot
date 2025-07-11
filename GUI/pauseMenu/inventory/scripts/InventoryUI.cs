using Godot;

public partial class InventoryUI : GridContainer
{
	// Exports
	[Export]
	private InventoryData data;

	// private
	private PackedScene inventorySlotScene = GD.Load<PackedScene>("res://GUI/pauseMenu/inventory/InventorySlot.tscn");
	private InventorySlot hoveredSlot;
	private EquipmentUI equipmentUI;

	// methods
	public override void _Ready()
	{
		equipmentUI = GetNode<EquipmentUI>("../../Equipment/HBoxContainer");

		// we cant use the pausemenu global static instance here because inventoryUI is a child of pausemenu
		// so its _Ready will be called before that of pausemenu (_Ready's call order is children then parent)
		PauseMenu pauseMenu = GetNode<PauseMenu>("../../../../../");
		pauseMenu.Connect(PauseMenu.SignalName.Shown, new(this, MethodName.UpdateInventory));
		pauseMenu.Connect(PauseMenu.SignalName.Hidden, new(this, MethodName.ClearInventory));
		pauseMenu.Connect(PauseMenu.SignalName.ItemRemoved, Callable.From(() => AddInventorySlot(this)));

		// inventorydata is a class so any instances will be reference types. assignments like below
		// will make both variables reference the same instance, so updating either will update both.
		// because of this, we can update playerinventory as much as we want and data will also have the same value!
		InitializeInventory();
		GlobalPlayerManager.Instance.PlayerInventory = data;
	}

	private void AddInventorySlot(Node parent)
	{
		InventorySlot slot = (InventorySlot)inventorySlotScene.Instantiate();
		parent.AddChild(slot);
	}

	private void InitializeInventory()
	{
		data.Slots ??= new();

		for (int i = data.Slots.Count; i < InventoryData.MAX_ITEMS; i++)
			data.Slots.Add(null);
	}

	private void ClearInventory()
	{
		foreach (Node child in GetChildren())
			child.QueueFree();

		equipmentUI.ClearInventory();
	}

	private void UpdateInventory()
	{
		foreach (SlotData data in data.Slots)
		{
			// here we need to add child slot first before setting slotData because slotData's setter
			// requires texture and label data that will only be loaded once its _Ready runs
			InventorySlot slot = (InventorySlot)inventorySlotScene.Instantiate();
			AddChild(slot);
			slot.SlotData = data;
			slot.Connect(InventorySlot.SignalName.EquipmentSelected, new(this, MethodName.OnEquipmentSelected));
			slot.Connect(InventorySlot.SignalName.EquipmentFocused, new(this, MethodName.OnEquipmentFocused));
			slot.Connect(BaseButton.SignalName.ButtonUp, Callable.From(() => OnInventorySlotButtonUp(slot)));
			slot.Connect(Control.SignalName.MouseEntered, Callable.From(() => OnInventorySlotMouseEntered(slot)));
		}

		equipmentUI.UpdateInventory();

		GetChildOrNull<Button>(0)?.GrabFocus();
	}

	private void OnInventorySlotMouseEntered(InventorySlot inventorySlot)
	{
		hoveredSlot = inventorySlot;
	}

	private void OnInventorySlotButtonUp(InventorySlot inventorySlot)
	{
		if (inventorySlot.Dragging && hoveredSlot != null && (hoveredSlot.SlotData == null || hoveredSlot.SlotData.Item.Name != inventorySlot.SlotData.Item.Name))
		{
			int hoveredIndex = hoveredSlot.GetIndex();
			int inventorySlotIndex = inventorySlot.GetIndex();
			(data.Slots[hoveredIndex], data.Slots[inventorySlotIndex]) = (data.Slots[inventorySlotIndex], data.Slots[hoveredIndex]);
			(inventorySlot.SlotData, hoveredSlot.SlotData) = (hoveredSlot.SlotData, inventorySlot.SlotData);
			hoveredSlot.GrabFocus();
		}

		inventorySlot.OnButtonUp();
	}

	private void OnEquipmentSelected(EquipableItem item)
	{
		int index = -1;

		for (int i = 0; i < data.Slots.Count; i++)
		{
			if (data.Slots[i] != null && data.Slots[i].Item.Name == item.Name)
			{
				index = i;
				break;
			}
		}

		int equipmentIndex = OnEquipmentFocused(item, true);
		InventorySlot inventorySlot = GetChild<InventorySlot>(index);
		InventorySlot equipmentSlot = equipmentUI.EquipmentContainers[equipmentIndex].GetChild<InventorySlot>(1);

		if (equipmentSlot.SlotData == null)
		{
			equipmentSlot.SlotData = inventorySlot.SlotData;
			equipmentUI.Equipment.Slots[equipmentIndex] = data.Slots[index];
			data.Slots.RemoveAt(index);
			inventorySlot.QueueFree();
			AddInventorySlot(this);
			GetChildOrNull<Button>((index + 1) % InventoryData.MAX_ITEMS)?.GrabFocus();
		}
		else
		{
			(data.Slots[index], equipmentUI.Equipment.Slots[equipmentIndex]) = (equipmentUI.Equipment.Slots[equipmentIndex], data.Slots[index]);
			(equipmentSlot.SlotData, inventorySlot.SlotData) = (inventorySlot.SlotData, equipmentSlot.SlotData);
			EquipableItem equipableItem = (EquipableItem)inventorySlot.SlotData.Item;
			PauseMenu.Instance.UpdateDescription(inventorySlot.SlotData.Item.Description + "\n\n" + equipableItem.StatsDescription);
			OnEquipmentFocused(equipableItem, false);
		}

		PauseMenu.Instance.EmitSignal(PauseMenu.SignalName.EquipmentsChanged, item);
	}

	private int OnEquipmentFocused(EquipableItem item, bool update)
	{
		int equipmentIndex = (int)item.EquipmentType;
		InventorySlot equipmentSlot = equipmentUI.EquipmentContainers[equipmentIndex].GetChild<InventorySlot>(1);

		int[] currentEquipment = equipmentSlot.SlotData == null ? new int[4] { 0, 0, 0, 0 } : EquipmentUI.CalculateSingleModifier((EquipableItem)equipmentSlot.SlotData.Item);
		int[] newEquipment = EquipmentUI.CalculateSingleModifier(item);

		for (int i = 0; i < 4; i++)
			newEquipment[i] -= currentEquipment[i];

		if (update)
			PauseMenu.Instance.Stats.UpdateStats(newEquipment[0], newEquipment[1]);
		else
			PauseMenu.Instance.Stats.ModifyStats(newEquipment[0], newEquipment[1]);

		return equipmentIndex;
	}
}
