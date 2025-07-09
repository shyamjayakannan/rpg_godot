using Godot;
using Godot.Collections;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(DialogChoiceResource), "res://GUI/dialogSystem/icons/question_bubble.png", nameof(Resource))]
public partial class DialogChoiceResource : DialogItemResource
{
	// Exports
	[Export]
	public Array<DialogBranchResource> DialogBranchResources = new();
}
