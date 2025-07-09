using Godot;
using Godot.Collections;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(DialogInteractionResource), "res://GUI/dialogSystem/icons/chat_bubbles.png", nameof(Resource))]
public partial class DialogInteractionResource : Resource
{
    // Exports
    [Export]
    public Array<DialogItemResource> DialogItemResources { get; private set; } = new();
}
