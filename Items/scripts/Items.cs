using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(Items), "", nameof(Resource))]
public class Items : Resource
{
	// Exports
	[Export]
	public string Name { get; private set; } = "";
	[Export(PropertyHint.MultilineText)]
	public string Description { get; protected set; } = "";
	[Export]
	public Texture Texture { get; private set; }
	[Export]
	private readonly ItemEffects[] effects;

	// methods
	public virtual void Use()
	{
		if (effects.Length == 0)
			return;

		foreach (ItemEffects effect in effects)
			effect.Use();
	}
}
