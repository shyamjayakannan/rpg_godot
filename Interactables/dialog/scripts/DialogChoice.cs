using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(DialogChoice), "res://GUI/dialogSystem/icons/question_bubble.png", nameof(Node2D))]
public class DialogChoice : DialogItem
{
	// methods
	public override string _GetConfigurationWarning()
	{
		int atLeastTwoValidChildren = 0;

		foreach (Node child in GetChildren())
			if (child is DialogBranch)
				atLeastTwoValidChildren++;

		if (atLeastTwoValidChildren < 2)
			return "please add at least two DialogBranch as child";
		else if (atLeastTwoValidChildren > 4)
			return "please add at most four DialogBranch as child";
		else
			return "";
	}

	public override void _Notification(int what)
	{
		if (what == NotificationChildOrderChanged)
		{
			DialogChoiceResource dialogChoiceResource = (DialogChoiceResource)DialogItemResource;
			dialogChoiceResource.DialogBranchResources.Clear();

			foreach (Node child in GetChildren())
				if (child is DialogBranch dialogItem)
					dialogChoiceResource.DialogBranchResources.Add((DialogBranchResource)dialogItem.DialogItemResource);
		}
	}

	public override void SetEditorDisplay()
	{
		base.SetEditorDisplay();
		ExampleSystem?.SetChoiceDisplay(((DialogChoiceResource)DialogItemResource).DialogBranchResources);
	}
}
