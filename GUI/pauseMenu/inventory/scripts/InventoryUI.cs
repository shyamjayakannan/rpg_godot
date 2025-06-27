using System.Collections.Generic;
using Godot;

public class InventoryUI : GridContainer
{
	// Exports
	[Export]
	private readonly InventoryData data;
	[Export]
	private readonly InventoryData equipment;

	// private
	private readonly PackedScene inventorySlotScene = GD.Load<PackedScene>("res://GUI/pauseMenu/inventory/InventorySlot.tscn");
	private HBoxContainer[] equipmentContainers;
	private InventorySlot hoveredSlot;

	// methods
	public override async void _Ready()
	{
		equipmentContainers = new HBoxContainer[4]
		{
			GetNode<HBoxContainer>("../../Equipment/HBoxContainer/VBoxContainer/HBoxContainer2"),
			GetNode<HBoxContainer>("../../Equipment/HBoxContainer/VBoxContainer/HBoxContainer"),
			GetNode<HBoxContainer>("../../Equipment/HBoxContainer/VBoxContainer2/HBoxContainer"),
			GetNode<HBoxContainer>("../../Equipment/HBoxContainer/VBoxContainer2/HBoxContainer2")
		};

		// we cant use the pausemenu global static instance here because inventoryUI is a child of pausemenu
		// so its _Ready will be called before that of pausemenu (_Ready's call order is children then parent)
		PauseMenu pauseMenu = GetNode<PauseMenu>("../../../../../");
		pauseMenu.Connect(nameof(PauseMenu.Shown), this, nameof(UpdateInventory));
		pauseMenu.Connect(nameof(PauseMenu.Hidden), this, nameof(ClearInventory));
		pauseMenu.Connect(nameof(PauseMenu.ItemRemoved), this, nameof(AddInventorySlot), new Godot.Collections.Array(this));
		GlobalSaveManager.Instance.Connect(nameof(GlobalSaveManager.GameLoaded), this, nameof(OnGameLoaded));

		// inventorydata is a class so any instances will be reference types. assignments like below
		// will make both variables reference the same instance, so updating either will update both.
		// because of this, we can update playerinventory as much as we want and data will also have the same value!
		data.Slots = new List<SlotData>(InventoryData.MAX_ITEMS);
		GlobalPlayerManager.Instance.PlayerInventory = data;

		for (int i = 0; i < InventoryData.MAX_ITEMS; i++)
			data.Slots.Add(null);

		InitializeEquipment();
		GlobalPlayerManager.Instance.PlayerEquipmentInventory = equipment;

		// so that pausemenu.stats exists (after pausemenu's ready call)
		await ToSignal(GetTree(), "idle_frame");

		int[] statModifiers = CalculateModifiers();
		pauseMenu.Stats.UpdateStats(statModifiers[0], statModifiers[1]);
	}

	private void OnGameLoaded()
	{
		InitializeEquipment();
		int[] statModifiers = CalculateModifiers();
		PauseMenu.Instance.Stats.UpdateStats(statModifiers[0], statModifiers[1]);
	}

	private void InitializeEquipment()
	{
		List<SlotData> list = new List<SlotData> { null, null, null, null };

		foreach (var slot in equipment.Slots)
		{
			if (slot?.Item is EquipableItem eq)
			{
				int idx = (int)eq.EquipmentType;

				if (list[idx] == null)
					list[idx] = slot;
			}
		}

		equipment.Slots = list;
	}

	private void ClearInventory()
	{
		foreach (Node child in GetChildren())
			child.QueueFree();

		foreach (HBoxContainer hBoxContainer in equipmentContainers)
			hBoxContainer.GetChildOrNull<InventorySlot>(1)?.QueueFree();
	}

	private void UpdateInventory()
	{
		foreach (SlotData data in data.Slots)
		{
			// here we need to add child slot first before setting slotData because slotData's setter
			// requires texture and label data that will only be loaded once its _Ready runs
			InventorySlot slot = (InventorySlot)inventorySlotScene.Instance();
			AddChild(slot);
			slot.SlotData = data;
			slot.Connect(nameof(InventorySlot.EquipmentSelected), this, nameof(OnEquipmentSelected));
			slot.Connect(nameof(InventorySlot.EquipmentFocused), this, nameof(OnEquipmentFocused));
			slot.Connect("button_up", this, nameof(OnInventorySlotButtonUp), new Godot.Collections.Array(slot));
			slot.Connect(nameof(InventorySlot.MouseEntered), this, nameof(OnInventorySlotMouseEntered), new Godot.Collections.Array(slot));
		}

		foreach (SlotData data in equipment.Slots)
		{
			InventorySlot slot = (InventorySlot)inventorySlotScene.Instance();

			// here we need to add child slot first before setting slotData because slotData's setter
			// requires texture and label data that will only be loaded once its _Ready runs
			if (data is null)
				continue;

			equipmentContainers[(int)((EquipableItem)data.Item).EquipmentType].AddChild(slot);
			slot.SlotData = data;
			slot.Disabled = true;
		}

		foreach (HBoxContainer hBoxContainer in equipmentContainers)
			if (hBoxContainer.GetChildCount() < 2)
				AddInventorySlot(hBoxContainer);

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

	private int OnEquipmentFocused(EquipableItem item, bool update)
	{
		int equipmentIndex = (int)item.EquipmentType;
		InventorySlot equipmentSlot = equipmentContainers[equipmentIndex].GetChild<InventorySlot>(1);

		int[] currentEquipment = equipmentSlot.SlotData is null ? new int[4] { 0, 0, 0, 0 } : CalculateSingleModifier((EquipableItem)equipmentSlot.SlotData.Item);
		int[] newEquipment = CalculateSingleModifier(item);

		for (int i = 0; i < 4; i++)
			newEquipment[i] -= currentEquipment[i];

		if (update)
			PauseMenu.Instance.Stats.UpdateStats(newEquipment[0], newEquipment[1]);
		else
			PauseMenu.Instance.Stats.ModifyStats(newEquipment[0], newEquipment[1]);

		return equipmentIndex;
	}

	public void AddInventorySlot(Node parent)
	{
		InventorySlot slot = (InventorySlot)inventorySlotScene.Instance();
		parent.AddChild(slot);
	}

	private void OnEquipmentSelected(EquipableItem item)
	{
		int index = data.Slots.FindIndex(slot => slot != null && slot.Item.Name == item.Name);
		int equipmentIndex = OnEquipmentFocused(item, true);
		InventorySlot inventorySlot = GetChild<InventorySlot>(index);
		InventorySlot equipmentSlot = equipmentContainers[equipmentIndex].GetChild<InventorySlot>(1);

		if (equipmentSlot.SlotData == null)
		{
			equipmentSlot.SlotData = inventorySlot.SlotData;
			equipment.Slots[equipmentIndex] = data.Slots[index];
			data.Slots.RemoveAt(index);
			inventorySlot.QueueFree();
			AddInventorySlot(this);
			GetChildOrNull<Button>((index + 1) % InventoryData.MAX_ITEMS)?.GrabFocus();
		}
		else
		{
			(data.Slots[index], equipment.Slots[equipmentIndex]) = (equipment.Slots[equipmentIndex], data.Slots[index]);
			(equipmentSlot.SlotData, inventorySlot.SlotData) = (inventorySlot.SlotData, equipmentSlot.SlotData);
			EquipableItem equipableItem = (EquipableItem)inventorySlot.SlotData.Item;
			PauseMenu.Instance.UpdateDescription(inventorySlot.SlotData.Item.Description + "\n\n" + equipableItem.StatsDescription);
			OnEquipmentFocused(equipableItem, false);
		}

		PauseMenu.Instance.EmitSignal(nameof(PauseMenu.EquipmentChanged), item);
	}

	private int[] CalculateModifiers()
	{
		int[] modifiers = new int[4] { 0, 0, 0, 0 };

		foreach (SlotData data in equipment.Slots)
		{
			if (data is null)
				continue;

			foreach (EquipableItemModifier modifier in ((EquipableItem)data.Item).Modifiers)
				modifiers[(int)modifier.EquipmentType] += modifier.Value;
		}

		return modifiers;
	}

	private int[] CalculateSingleModifier(EquipableItem equipableItem)
	{
		int[] modifiers = new int[4] { 0, 0, 0, 0 };

		foreach (EquipableItemModifier modifier in equipableItem.Modifiers)
			modifiers[(int)modifier.EquipmentType] += modifier.Value;

		return modifiers;
	}
}
