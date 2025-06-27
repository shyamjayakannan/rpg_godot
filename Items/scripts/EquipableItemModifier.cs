using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(EquipableItemModifier), "", nameof(Resource))]
public class EquipableItemModifier : Resource
{
    // Exports
    [Export]
    public Type EquipmentType
    {
        get => equipmentType;
        set
        {
            equipmentType = value;

            if (Engine.EditorHint)
                EmitSignal("changed");
        }
    }
    [Export]
    public int Value
    {
        get => _value;
        set
        {
            _value = value;

            if (Engine.EditorHint)
                EmitSignal("changed");
        }
    }

    // private
    private Type equipmentType = Type.Health;
    private int _value = 1;

    // properties
    public enum Type
    {
        Attack,
        Defence,
        Health,
        Speed
    }
}
