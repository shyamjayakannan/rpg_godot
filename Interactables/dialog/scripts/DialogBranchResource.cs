using System.Collections.Generic;
using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(DialogBranchResource), "res://GUI/dialogSystem/icons/answer_bubble.png", nameof(Resource))]
public class DialogBranchResource : DialogItemResource
{
	// Exports
	[Export]
	public string Text
	{
		get => text;
		set
		{
			text = value;
			EmitSignal("changed");
		}
	}
	[Export]
	public List<DialogItemResource> DialogItemResources { get; private set; } = new List<DialogItemResource>();

	// private
	private string text = "Ok...";
}
