using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(QuestConditionResource), "res://Quests/utilityNodes/icons/quest_switch.png", nameof(Resource))]
public class QuestConditionResource : QuestNodeResource
{
    // Exports
    [Export]
    private CheckType CheckTypeInstance
    {
        get => checkType;
        set
        {
            checkType = value;
            UpdateSummary();
        }
    }
    [Export]
    private readonly bool removeWhenActivated = false;

    // private
    private CheckType checkType = CheckType.HasQuest;
    private enum CheckType
    {
        HasQuest,
        QuestStepComplete,
        OnCurrentQuestStep,
        QuestComplete,
        HasItem
    }

    // methods
    public bool CheckIsActivated()
    {
        GlobalSaveManager.QuestData questData = GlobalQuestManager.Instance.FindQuest(LinkedQuest);

        if (questData.Title == "not found")
            return SetIsActivated(false);

        switch (CheckTypeInstance)
        {
            case CheckType.HasQuest:
                return SetIsActivated(true);

            case CheckType.QuestComplete:
                return SetIsActivated(questData.IsComplete);

            case CheckType.QuestStepComplete:
                return SetIsActivated(QuestStep > 0 && questData.CompletedSteps.Contains(GetStep().Step));

            case CheckType.OnCurrentQuestStep:
                string step = GetStep().Step;

                if (step == "N/A" || questData.CompletedSteps.Contains(step))
                    return SetIsActivated(false);

                string previousStep = QuestStep <= LinkedQuest.Steps.Length && QuestStep > 1 ? LinkedQuest.Steps[QuestStep - 2].Step : "N/A";
                return SetIsActivated(previousStep == "N/A" || questData.CompletedSteps.Contains(previousStep));

            case CheckType.HasItem:
                QuestResource questResource = GlobalQuestManager.Instance.FindQuestByTitle(LinkedQuest.Title);

                foreach (QuestStepResource questStepResource in questResource.Steps)
                    if (questStepResource is ItemDeliverQuestStepResource itemDeliverQuestStepResource && GlobalPlayerManager.Instance.PlayerInventory.GetQuantity(itemDeliverQuestStepResource.Item) > 0)
                        return SetIsActivated(true);

                return SetIsActivated(false);

            default:
                return false;
        }
    }

    private bool SetIsActivated(bool isActivated)
    {
        return isActivated ? !removeWhenActivated : removeWhenActivated;
    }

    protected override void UpdateSummary()
    {
        SettingsSummary = $"UPDATE QUEST\nQuest: {LinkedQuest?.Title}\nChecking whether ";

        switch (CheckTypeInstance)
        {
            case CheckType.HasQuest:
                SettingsSummary += "player has quest";
                break;

            case CheckType.QuestStepComplete:
                SettingsSummary += $"player has completed step: {GetStep()}";
                break;

            case CheckType.OnCurrentQuestStep:
                SettingsSummary += $"player is on step: {GetStep()}";
                break;

            case CheckType.HasItem:
                SettingsSummary += $"player has the required items";
                break;

            case CheckType.QuestComplete:
                SettingsSummary += "quest is complete";
                break;
        }

        // needed
        PropertyListChangedNotify();
    }
}
