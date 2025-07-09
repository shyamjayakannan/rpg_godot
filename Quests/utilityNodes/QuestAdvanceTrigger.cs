using Godot;

[Tool]
[GlobalClass, Icon("res://Quests/utilityNodes/icons/quest_advance.png")]
public partial class QuestAdvanceTrigger : QuestNode
{
    // Exports
    [Export]
    private string signal;

    // methods
    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            return;

        GetNode<Sprite2D>("Sprite2D").QueueFree();
        GetParent().Connect(signal, new(this, MethodName.AdvanceQuest));
    }

    private void AdvanceQuest()
    {
        if (LinkedQuest == null)
            return;

        string step = GetStep();
        GlobalQuestManager.Instance.UpdateQuest(LinkedQuest.Title, LinkedQuest, step == "N/A" ? "" : step);
    }
}
