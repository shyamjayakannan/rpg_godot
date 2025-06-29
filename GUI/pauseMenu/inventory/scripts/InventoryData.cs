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
		if (item is EquipableItem)
		{
			(int index1, int firstNullIndex1) = GlobalPlayerManager.Instance.PlayerInventory.SearchItem(item);
			(int index2, int firstNullIndex2) = GlobalPlayerManager.Instance.PlayerEquipmentInventory.SearchItem(item);

			if (index1 >= 0 || index2 >= 0)
				return false;

			if (firstNullIndex1 >= 0)
			{
				Slots[firstNullIndex1] = new SlotData(quantity, item);
				return true;
			}

			if (firstNullIndex2 >= 0)
			{
				Slots[firstNullIndex2] = new SlotData(quantity, item);
				return true;
			}

			return false;
		}

		(int index, int firstNullIndex) = SearchItem(item);

		if (index >= 0)
		{
			Slots[index].Quantity += quantity;
			return true;
		}

		if (firstNullIndex < 0)
			return false;

		Slots[firstNullIndex] = new SlotData(quantity, item);
		return true;
	}

	public bool IsEquipmentPresent(EquipableItem equipableItem)
	{
		(int index, int _) = SearchItem(equipableItem);
		return index >= 0;
	}

	public bool RemoveItem(Items item, int quantity = 1)
	{
		(int index, int _) = SearchItem(item);

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
		(int index, _) = SearchItem(item);

		return index < 0 ? 0 : Slots[index].Quantity;
	}

	private (int, int) SearchItem(Items item)
	{
		int firstNullIndex = -1;

		for (int i = 0; i < Slots.Count; i++)
		{
			if (Slots[i] == null)
			{
				if (firstNullIndex == -1)
					firstNullIndex = i;

				continue;
			}

			if (item.Name == Slots[i].Item.Name)
				return (i, -1); // last two don't matter here
		}

		return (-1, firstNullIndex);
	}
}
