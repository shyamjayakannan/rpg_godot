using Godot;

[Tool]
public class QuestNode : Node2D
{
    // Exports
    [Export]
    protected QuestResource LinkedQuest
    {
        get => linkedQuest;
        set
        {
            linkedQuest = value;
            QuestStep = 0;
            UpdateSummary();
        }
    }
    [Export]
    protected int QuestStep
    {
        get => questStep;
        set
        {
            questStep = Mathf.Clamp(value, 0, LinkedQuest == null ? 0 : LinkedQuest.Steps.Length);
            UpdateSummary();
        }
    }
    [Export(PropertyHint.MultilineText)]
    protected string SettingsSummary { get; set; }

    // private
    private QuestResource linkedQuest = null;
    private int questStep = 0;

    // methods
    protected virtual void UpdateSummary()
    {
        SettingsSummary = $"UPDATE QUEST\nQuest: {LinkedQuest.Title}\nStep: {QuestStep} - {GetStep()}\nComplete: {questStep == linkedQuest.Steps.Length}";

        // needed
        PropertyListChangedNotify();
    }

    protected string GetStep()
    {
        return QuestStep != 0 ? LinkedQuest.Steps[QuestStep - 1] : "N/A";
    }
}
