using Godot;
using Godot.Collections;

namespace Rpg
{
	[Tool]
	[GlobalClass, Icon("res://GUI/dialogSystem/icons/question_bubble.png")]
	public partial class DialogChoiceResource : DialogItemResource
	{
		// Exports
		[Export]
		public Array<DialogBranchResource> DialogBranchResources = new();
	}
}
