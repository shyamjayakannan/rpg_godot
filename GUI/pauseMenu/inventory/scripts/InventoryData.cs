using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Rpg
{
	[GlobalClass]
	public partial class InventoryData : Resource
	{
		// Exports
		[Export]
		public Array<SlotData> Slots { get; set; }

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
					Slots[firstNullIndex1] = new(quantity, item);
					return true;
				}

				if (firstNullIndex2 >= 0)
				{
					Slots[firstNullIndex2] = new(quantity, item);
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

			Slots[firstNullIndex] = new(quantity, item);
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
			return Slots.Select(slot =>
			{
				if (slot == null)
					return new GlobalSaveManager.ItemData
					{
						Path = "",
						Quantity = 0
					};
				else
					return new GlobalSaveManager.ItemData
					{
						Path = slot.Item.ResourcePath,
						Quantity = slot.Quantity
					};
			}).ToList();
		}

		public void SetSaveData(List<GlobalSaveManager.ItemData> items)
		{
			Array<SlotData> slots = new();

			foreach (GlobalSaveManager.ItemData item in items)
			{
				if (item.Path == "")
				{
					slots.Add(null);
					continue;
				}

				slots.Add(new(
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
}
