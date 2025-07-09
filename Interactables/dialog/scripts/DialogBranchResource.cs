using Godot;
using Godot.Collections;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(DialogBranchResource), "res://GUI/dialogSystem/icons/answer_bubble.png", nameof(Resource))]
public partial class DialogBranchResource : DialogItemResource
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
	public Array<DialogItemResource> DialogItemResources { get; private set; } = new();

	// private
	private string text = "Ok...";
}
