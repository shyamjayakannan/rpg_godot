using System.Collections.Generic;
using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(DialogChoiceResource), "res://GUI/dialogSystem/icons/question_bubble.png", nameof(Resource))]
public class DialogChoiceResource : DialogItemResource
{
	// Exports
	[Export]
	public List<DialogBranchResource> DialogBranchResources = new List<DialogBranchResource>();
}
