using System.Collections.Generic;
using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(DialogInteractionResource), "res://GUI/dialogSystem/icons/chat_bubbles.png", nameof(Resource))]
public class DialogInteractionResource : Resource
{
    // Exports
    [Export]
    public List<DialogItemResource> DialogItemResources { get; private set; } = new List<DialogItemResource>();
}
