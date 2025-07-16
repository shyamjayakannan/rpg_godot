using Godot;
using Godot.Collections;

namespace Rpg
{
	[Tool]
	[GlobalClass, Icon("res://GUI/dialogSystem/icons/answer_bubble.png")]
	public partial class DialogBranchResource : DialogItemResource
	{
		// Exports
		[Export]
		public string Text
		{
			get => text;
			set
			{
				text = value;
				EmitSignal(Resource.SignalName.Changed);
			}
		}
		[Export]
		public Array<DialogItemResource> DialogItemResources { get; private set; } = new();

		// private
		private string text = "Ok...";
	}
}
