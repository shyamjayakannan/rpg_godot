using System.Collections.Generic;
using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(DialogBranch), "res://GUI/dialogSystem/icons/answer_bubble.png", nameof(Node2D))]
public class DialogBranch : DialogItem
{
	// Signals
	[Signal]
	public delegate void Selected(string text);

	// Exports
	[Export]
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
	private string text = "Ok...";

	// properties
	public List<DialogItem> DialogItems { get; private set; } = new List<DialogItem>();

	// methods
	public override void _Ready()
	{
		base._Ready();

		if (Engine.EditorHint)
			return;

		foreach (Node child in GetChildren())
			if (child is DialogItem dialogItem)
				DialogItems.Add(dialogItem);
	}

	// public override void SetEditorDisplay()
	// {
	// 	// cannot call choicedialog's seteditordisplay because each one's examplesystem is different
	// 	// and is cleared and created on selecting in the editor. so has to be called on own examplesystem.
	// 	ExampleSystem.SetChoiceDisplay(((DialogChoice)GetParent()).DialogBranches);
	// }
}
