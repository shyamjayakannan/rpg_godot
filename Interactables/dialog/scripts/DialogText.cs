using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(DialogText), "res://GUI/dialogSystem/icons/text_bubble.png", nameof(Node2D))]
public class DialogText : DialogItem
{
	// methods
	public override void SetEditorDisplay()
	{
		base.SetEditorDisplay();
		ExampleSystem?.SetTextDisplay(((DialogTextResource)DialogItemResource).Text);
	}
}
