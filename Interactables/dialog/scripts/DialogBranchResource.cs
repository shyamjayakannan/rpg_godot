using System.Collections.Generic;
using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(DialogBranchResource), "res://GUI/dialogSystem/icons/answer_bubble.png", nameof(Resource))]
public class DialogBranchResource : DialogItemResource
{
	// Signals
	[Signal]
	public delegate void Selected();

	// Exports
	[Export]
	public string Text { get; private set; } = "Ok...";
	[Export]
	public List<DialogItemResource> DialogItemResources { get; private set; } = new List<DialogItemResource>();
}
