using System.Collections.Generic;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(InventoryData), "", nameof(Resource))]
public class InventoryData : Resource
{
	// Exports
	[Export]
	public List<SlotData> Slots { get; set; }

	// properties
	public const int MAX_ITEMS = 20;

	// methods
	public bool AddItem(Items item, int quantity = 1)
	{
		(int index, bool full, int firstNullIndex) = SearchItem(item);

		if (index >= 0)
		{
			if (item is EquipableItem)
			{
				PlayerHUD.Instance.QueueNotification("Equipment Duplicate Found!", "This equipment is already present in the inventory.");
				return false;
			}

			Slots[index].Quantity += quantity;
			return true;
		}

		if (full)
			return false;

		Slots[firstNullIndex] = new SlotData(quantity, item);
		return true;
	}

	public bool RemoveItem(Items item, int quantity = 1)
	{
		(int index, bool _, int _) = SearchItem(item);

		if (index >= 0)
		{
			Slots[index].Quantity -= quantity;

			if (Slots[index].Quantity == 0)
				Slots[index] = null;

			return true;
		}

		return false;
	}

	public List<GlobalSaveManager.ItemData> GetSaveData()
	{
		List<GlobalSaveManager.ItemData> list = new List<GlobalSaveManager.ItemData>();

		foreach (SlotData slot in Slots)
		{
			if (slot == null)
			{
				list.Add(new GlobalSaveManager.ItemData
				{
					Path = "",
					Quantity = 0
				});

				continue;
			}

			list.Add(new GlobalSaveManager.ItemData
			{
				Path = slot.Item.ResourcePath,
				Quantity = slot.Quantity
			});
		}

		return list;
	}

	public void SetSaveData(List<GlobalSaveManager.ItemData> items)
	{
		List<SlotData> slots = new List<SlotData>();

		foreach (GlobalSaveManager.ItemData item in items)
		{
			if (item.Path == "")
			{
				slots.Add(null);
				continue;
			}

			slots.Add(new SlotData(
				item.Quantity,
				GD.Load<Items>(item.Path)
			));
		}

		Slots = slots;
	}

	public int GetQuantity(Items item)
	{
		(int index, _, _) = SearchItem(item);

		return index < 0 ? 0 : Slots[index].Quantity;
	}

	private (int, bool, int) SearchItem(Items item)
	{
		bool full = true;
		int firstNullIndex = -1;

		for (int i = 0; i < Slots.Count; i++)
		{
			if (Slots[i] == null)
			{
				full = false;

				if (firstNullIndex == -1)
					firstNullIndex = i;

				continue;
			}

			if (item.Name == Slots[i].Item.Name)
				return (i, false, -1); // last two don't matter here
		}

		return (-1, full, firstNullIndex);
	}
}
