using Godot;

namespace Rpg
{
	[Tool]
	[GlobalClass, Icon("res://GUI/dialogSystem/icons/text_bubble.png")]
	public partial class DialogText : DialogItem
	{
		// methods
		public override void SetEditorDisplay()
		{
			base.SetEditorDisplay();
			ExampleSystem?.SetTextDisplay(((DialogTextResource)DialogItemResource).Text);
		}
	}
}
