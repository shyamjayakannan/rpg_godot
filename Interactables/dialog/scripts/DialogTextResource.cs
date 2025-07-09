using Godot;

[Tool]
[GlobalClass, Icon("res://GUI/dialogSystem/icons/text_bubble.png")]
public partial class DialogTextResource : DialogItemResource
{
	// Exports
	[Export(PropertyHint.MultilineText)]
	public string Text
	{
		get => text;
		set
		{
			text = value;
			EmitSignal(Resource.SignalName.Changed);
		}
	}

	// private
	private string text;
}
