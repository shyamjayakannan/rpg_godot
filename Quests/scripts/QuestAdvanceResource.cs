using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(QuestAdvanceResource), "res://Quests/utilityNodes/icons/quest_advance.png", nameof(Resource))]
public partial class QuestAdvanceResource : QuestNodeResource
{
    // methods
    public void AdvanceQuest()
    {
        if (LinkedQuest == null)
            return;

        QuestStepResource step = GetStep();

        // this step can be any of the itemdeliver steps in the quest. we will check and update all
        if (step is not ItemDeliverQuestStepResource)
        {
            GlobalQuestManager.Instance.UpdateQuest(LinkedQuest.Title, LinkedQuest, step.Step == "N/A" ? "" : step.Step);
            return;
        }

        foreach (QuestStepResource questStepResource in LinkedQuest.Steps)
        {
            if (questStepResource is ItemDeliverQuestStepResource itemDeliverQuestStepResource)
            {
                int inInventory = GlobalPlayerManager.Instance.PlayerInventory.GetQuantity(itemDeliverQuestStepResource.Item);

                if (inInventory == 0)
                    continue;

                GlobalQuestManager.Instance.UpdateItemDeliverSteps(LinkedQuest.Title, LinkedQuest, inInventory, itemDeliverQuestStepResource);
            }
        }
    }
}
