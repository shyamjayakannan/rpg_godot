using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(EquipableItemModifier), "", nameof(Resource))]
public partial class EquipableItemModifier : Resource
{
    // Exports
    [Export]
    public Type EquipmentType
    {
        get => equipmentType;
        set
        {
            equipmentType = value;

            if (Engine.IsEditorHint())
                EmitSignal(Resource.SignalName.Changed);
        }
    }
    [Export]
    public int Value
    {
        get => _value;
        set
        {
            _value = value;

            if (Engine.IsEditorHint())
                EmitSignal(Resource.SignalName.Changed);
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
