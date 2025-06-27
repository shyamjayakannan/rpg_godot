using System.Collections.Generic;
using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(DialogChoice), "res://GUI/dialogSystem/icons/question_bubble.png", nameof(Node2D))]
public class DialogChoice : DialogItem
{
	// properties
	public List<DialogBranch> DialogBranches
	{
		get => dialogBranches;
		set
		{
			dialogBranches = value;

			if (Engine.EditorHint && ExampleSystem != null)
				SetEditorDisplay();
		}
	}

	// private
	private List<DialogBranch> dialogBranches = new List<DialogBranch>();

	// method
	public override void _Ready()
	{
		base._Ready();

		foreach (Node child in GetChildren())
			if (child is DialogBranch dialogItem)
				DialogBranches.Add(dialogItem);
	}

	public override string _GetConfigurationWarning()
	{
		int atLeastTwoValidChildren = 0;

		foreach (Node child in GetChildren())
			if (child is DialogBranch)
				atLeastTwoValidChildren++;

		if (atLeastTwoValidChildren < 1)
			return "please add at least two DialogBranch as child";
		else if (atLeastTwoValidChildren > 4)
			return "please add at most four DialogBranch as child";
		else
			return "";
	}

	public override void SetEditorDisplay()
	{
		ExampleSystem.SetChoiceDisplay(DialogBranches);
	}
}
