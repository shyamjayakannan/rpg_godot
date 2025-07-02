using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(QuestAdvanceTrigger), "res://Quests/utilityNodes/icons/quest_advance.png", nameof(Node2D))]
public class QuestAdvanceTrigger : QuestNode
{
    // Exports
    [Export]
    private readonly string signal;

    // methods
    public override void _Ready()
    {
        if (Engine.EditorHint)
            return;

        GetNode<Sprite>("Sprite").QueueFree();
        GetParent().Connect(signal, this, nameof(AdvanceQuest));
    }

    private void AdvanceQuest()
    {
        if (LinkedQuest == null)
            return;

        string step = GetStep();
        GlobalQuestManager.Instance.UpdateQuest(LinkedQuest.Title, LinkedQuest, step == "N/A" ? "" : step);
    }
}
