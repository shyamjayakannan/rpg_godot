using Godot;

[Tool]
[GlobalClass, Icon("res://GUI/dialogSystem/icons/question_bubble.png")]
public partial class DialogChoice : DialogItem
{
	// methods
	public override string[] _GetConfigurationWarnings()
	{
		int atLeastTwoValidChildren = 0;

		foreach (Node child in GetChildren())
			if (child is DialogBranch)
				atLeastTwoValidChildren++;

		if (atLeastTwoValidChildren < 2)
			return new[] { "please add at least two DialogBranch as child" };
		else if (atLeastTwoValidChildren > 4)
			return new[] { "please add at most four DialogBranch as child" };
		else
			return System.Array.Empty<string>();
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
