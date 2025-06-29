using System.Collections.Generic;
using Godot;

public class EquipmentUI : HBoxContainer
{
    // Exports
    [Export]
    public InventoryData Equipment { get; private set; }

    // private
    private readonly PackedScene inventorySlotScene = GD.Load<PackedScene>("res://GUI/pauseMenu/inventory/InventorySlot.tscn");

    // properties
    public HBoxContainer[] EquipmentContainers { get; private set; }

    // methods
    public override async void _Ready()
    {
        EquipmentContainers = new HBoxContainer[4]
        {
            GetNode<HBoxContainer>("../../Equipment/HBoxContainer/VBoxContainer/HBoxContainer2"),
            GetNode<HBoxContainer>("../../Equipment/HBoxContainer/VBoxContainer/HBoxContainer"),
            GetNode<HBoxContainer>("../../Equipment/HBoxContainer/VBoxContainer2/HBoxContainer"),
            GetNode<HBoxContainer>("../../Equipment/HBoxContainer/VBoxContainer2/HBoxContainer2")
        };

        GlobalPlayerManager.Instance.PlayerEquipmentInventory = Equipment;

        GlobalSaveManager.Instance.Connect(nameof(GlobalSaveManager.GameLoaded), this, nameof(OnGameLoaded));

        // so that pausemenu.stats exists (after pausemenu's ready call)
        await ToSignal(GetTree(), "idle_frame");

        OnGameLoaded();
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

        foreach (var slot in Equipment.Slots)
        {
            if (slot?.Item is EquipableItem eq)
            {
                int idx = (int)eq.EquipmentType;

                if (list[idx] == null)
                {
                    list[idx] = slot;
                    PauseMenu.Instance.EmitSignal(nameof(PauseMenu.EquipmentsChanged), eq);
                }
            }
        }

        Equipment.Slots = list;
    }

    public void ClearInventory()
    {
        foreach (HBoxContainer hBoxContainer in EquipmentContainers)
            hBoxContainer.GetChildOrNull<InventorySlot>(1)?.QueueFree();
    }

    private int[] CalculateModifiers()
    {
        int[] modifiers = new int[4] { 0, 0, 0, 0 };

        foreach (SlotData data in Equipment.Slots)
        {
            if (data == null)
                continue;

            foreach (EquipableItemModifier modifier in ((EquipableItem)data.Item).Modifiers)
                modifiers[(int)modifier.EquipmentType] += modifier.Value;
        }

        return modifiers;
    }

    public void UpdateInventory()
    {
        foreach (SlotData data in Equipment.Slots)
        {
            InventorySlot slot = (InventorySlot)inventorySlotScene.Instance();

            // here we need to add child slot first before setting slotData because slotData's setter
            // requires texture and label data that will only be loaded once its _Ready runs
            if (data == null)
                continue;

            EquipmentContainers[(int)((EquipableItem)data.Item).EquipmentType].AddChild(slot);
            slot.SlotData = data;
            slot.Disabled = true;
        }

        foreach (HBoxContainer hBoxContainer in EquipmentContainers)
        {
            if (hBoxContainer.GetChildCount() < 2)
            {
                InventorySlot slot = (InventorySlot)inventorySlotScene.Instance();
                hBoxContainer.AddChild(slot);
            }
        }

        GetChildOrNull<Button>(0)?.GrabFocus();
    }

    public int[] CalculateSingleModifier(EquipableItem equipableItem)
    {
        int[] modifiers = new int[4] { 0, 0, 0, 0 };

        foreach (EquipableItemModifier modifier in equipableItem.Modifiers)
            modifiers[(int)modifier.EquipmentType] += modifier.Value;

        return modifiers;
    }
}
