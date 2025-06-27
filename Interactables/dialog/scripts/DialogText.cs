using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(DialogText), "res://GUI/dialogSystem/icons/text_bubble.png", nameof(Node2D))]
public class DialogText : DialogItem
{
	// Exports
	[Export(PropertyHint.MultilineText)]
	public string Text
	{
		get => text;
		set
		{
			text = value;

			if (Engine.EditorHint && ExampleSystem != null)
				SetEditorDisplay();
		}
	}

	// private
	private string text;

	// methods
	public override void SetEditorDisplay()
	{
		ExampleSystem.SetTextDisplay(Text);
	}
}
