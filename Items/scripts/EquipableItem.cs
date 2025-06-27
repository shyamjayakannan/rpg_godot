using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(EquipableItem), "", nameof(Resource))]
public class EquipableItem : Items
{
    // Exports
    [Export(PropertyHint.MultilineText)]
    public string StatsDescription { get; private set; }
    [Export]
    public Type EquipmentType
    {
        get => equipmentType;
        private set
        {
            equipmentType = value;

            if (Engine.EditorHint)
                UpdateDescription();
        }
    }
    [Export]
    public EquipableItemModifier[] Modifiers
    {
        get => modifiers;
        private set
        {
            if (!Engine.EditorHint)
            {
                modifiers = value;
                return;
            }

            // Disconnect old signals
            // need to do this because godot doesn't automatically disconnect signals for resources
            if (modifiers != null)
                foreach (EquipableItemModifier mod in modifiers)
                    if (mod != null && mod.IsConnected("changed", this, nameof(UpdateDescription)))
                        mod.Disconnect("changed", this, nameof(UpdateDescription));

            modifiers = value;

            // Connect new signals
            if (modifiers != null)
                foreach (EquipableItemModifier mod in modifiers)
                    mod?.Connect("changed", this, nameof(UpdateDescription));

            UpdateDescription();
        }
    }
    [Export]
    public Texture SpriteTexture { get; private set; }

    // private
    private EquipableItemModifier[] modifiers = new EquipableItemModifier[0];
    public Type equipmentType = Type.Weapon;

    // properties
    public enum Type
    {
        Weapon,
        Armor,
        Amulet,
        Ring
    }

    // methods
    public override void Use()
    {
        base.Use();
    }

    private void UpdateDescription()
    {
        int attackModifier = 0;
        int defenceModifier = 0;
        int healthModifier = 0;
        int speedModifier = 0;

        foreach (EquipableItemModifier modifier in Modifiers)
        {
            switch (modifier.EquipmentType)
            {
                case EquipableItemModifier.Type.Attack:
                    attackModifier += modifier.Value;
                    break;

                case EquipableItemModifier.Type.Defence:
                    defenceModifier += modifier.Value;
                    break;

                case EquipableItemModifier.Type.Health:
                    healthModifier += modifier.Value;
                    break;

                case EquipableItemModifier.Type.Speed:
                    speedModifier += modifier.Value;
                    break;
            }
        }

        StatsDescription = $"Type: {EquipmentType}";
        StatsDescription += $"\n\nAttack: {(attackModifier >= 0 ? $"+{attackModifier}" : attackModifier.ToString())}";
        StatsDescription += $"    Defence: {(defenceModifier >= 0 ? $"+{defenceModifier}" : defenceModifier.ToString())}";
        StatsDescription += $"\nHealth: {(healthModifier >= 0 ? $"+{healthModifier}" : healthModifier.ToString())}";
        StatsDescription += $"      Speed: {(speedModifier >= 0 ? $"+{speedModifier}" : speedModifier.ToString())}";

        PropertyListChangedNotify();
    }
}
