using Godot;

[Tool]
[GlobalClass, Icon("res://GUI/dialogSystem/icons/chat_bubble.png")]
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
