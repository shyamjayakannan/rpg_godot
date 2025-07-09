using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(Items), "", nameof(Resource))]
public partial class Items : Resource
{
	// Exports
	[Export]
	public string Name { get; private set; } = "";
	[Export(PropertyHint.MultilineText)]
	public string Description { get; protected set; } = "";
	[Export]
	public Texture2D Texture2D { get; private set; }
	[Export]
	private ItemEffects[] effects;
	[Export]
	public int Cost = 1;

	// methods
	public virtual void Use()
	{
		if (effects.Length == 0)
			return;

		foreach (ItemEffects effect in effects)
			effect.Use();
	}
}
