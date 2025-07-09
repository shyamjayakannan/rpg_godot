using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(DialogItemResource), "res://GUI/dialogSystem/icons/chat_bubble.png", nameof(Resource))]
public abstract partial class DialogItemResource : Resource
{
    //Exports
    [Export]
    public NpcResource NpcResource
    {
        get => npcResource;
        set
        {
            npcResource = value;
            EmitSignal(Resource.SignalName.Changed);
        }
    }
    [Export]
    public QuestConditionResource QuestConditionResource { get; set; }
    [Export]
    public QuestAdvanceResource QuestAdvanceResource { get; private set; }

    // private
    private NpcResource npcResource;
}
