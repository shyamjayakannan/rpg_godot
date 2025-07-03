using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(DialogTextResource), "res://GUI/dialogSystem/icons/text_bubble.png", nameof(Resource))]
public class DialogTextResource : DialogItemResource
{
	// Exports
	[Export(PropertyHint.MultilineText)]
	public string Text
	{
		get => text;
		set
		{
			text = value;
			EmitSignal("changed");
		}
	}

	// private
	private string text;
}
