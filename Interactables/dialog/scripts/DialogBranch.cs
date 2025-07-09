using Godot;

[GlobalClass, Icon("res://GUI/dialogSystem/icons/answer_bubble.png")]
public partial class DialogBranch : DialogItem
{
	// methods
	public override void _Ready()
	{
		base._Ready();

		// child duplication doesnt fire NotificationChildOrderChanged so need to send it manually
		GetParent()?.Notification((int)NotificationChildOrderChanged);
	}

	public override void SetEditorDisplay()
	{
		base.SetEditorDisplay();
		// cannot call choicedialog's seteditordisplay because each one's examplesystem is different
		// and is cleared and created on selecting in the editor. so has to be called on own examplesystem.
		ExampleSystem?.SetChoiceDisplay(((DialogChoiceResource)((DialogChoice)GetParent()).DialogItemResource).DialogBranchResources);
	}
}
