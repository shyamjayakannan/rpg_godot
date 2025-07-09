using Godot;
using Godot.Collections;

[Tool]
[GlobalClass, Icon("res://GUI/dialogSystem/icons/chat_bubbles.png")]
public partial class DialogInteractionResource : Resource
{
    // Exports
    [Export]
    public Array<DialogItemResource> DialogItemResources { get; private set; } = new();
}
