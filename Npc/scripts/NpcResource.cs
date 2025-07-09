using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(NpcResource), "", nameof(Resource))]
public partial class NpcResource : Resource
{
    // Exports
    [Export]
    public string Name { get; private set; } = "";
    [Export]
    public Texture2D Sprite2D { get; private set; }
    [Export]
    public Texture2D Portrait { get; private set; }
    [Export]
    public float DialoguePitch { get; private set; } = 1.0f;
}
