using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(ItemEffects), "", nameof(Resource))]
public abstract partial class ItemEffects : Resource
{
    // methods
    public abstract void Use();
}
