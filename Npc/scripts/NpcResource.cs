using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(NpcResource), "", nameof(Resource))]
public class NpcResource : Resource
{
    // Exports
    [Export]
    public string Name { get; private set; } = "";
    [Export]
    public Texture Sprite { get; private set; }
    [Export]
    public Texture Portrait { get; private set; }
    [Export]
    public float DialoguePitch { get; private set; } = 1.0f;
}
