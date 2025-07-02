using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(DialogItemResource), "res://GUI/dialogSystem/icons/chat_bubble.png", nameof(Resource))]
public abstract partial class DialogItemResource : Resource
{
    //Exports
    [Export]
    public NpcResource NpcResource { get; set; }
    [Export]
    public QuestConditionResource QuestConditionResource { get; set; }
    [Export]
    public QuestAdvanceResource QuestAdvanceResource { get; private set; }
}
