using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(ItemEffects), "", nameof(Resource))]
public abstract class ItemEffects : Resource
{
    // methods
    public abstract void Use();
}
