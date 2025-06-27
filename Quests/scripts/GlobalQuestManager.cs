using System.Collections.Generic;
using Godot;

public class GlobalQuestManager : Node
{
    // Signals
    [Signal]
    public delegate void QuestUpdated();

    // private
    private readonly List<QuestResource> quests = new List<QuestResource>();
    private const string QUEST_LOCATION = "res://Quests/quests";

    // properties
    public static GlobalQuestManager Instance { get; private set; }
    public List<GlobalSaveManager.QuestData> CurrentQuests { get; set; } = new List<GlobalSaveManager.QuestData>();

    // methods
    public override void _Ready()
    {
        Instance = this;
    }

    public void LoadQuests()
    {
        // Open the directory at QUEST_LOCATION and get all files
        Directory dir = new Directory();

        if (dir.Open(QUEST_LOCATION) != Error.Ok)
            return;

        dir.ListDirBegin(skipNavigational: true, skipHidden: true);
        string fileName = dir.GetNext();

        while (!string.IsNullOrEmpty(fileName))
        {
            if (dir.CurrentIsDir())
                continue;

            QuestResource questResource = GD.Load<QuestResource>($"{QUEST_LOCATION}/{fileName}");

            foreach (GlobalSaveManager.QuestData questData in CurrentQuests)
                if (questResource.Title == questData.Title)
                    quests.Add(questResource);

            fileName = dir.GetNext();
        }
        dir.ListDirEnd();
    }

    public void UpdateQuest(string title, QuestResource questResource, string completedStep = "")
    {
        int i = GetQuestIndexByTitle(title);

        if (i > -1)
        {
            if (completedStep != "" && !CurrentQuests[i].CompletedSteps.Contains(completedStep))
                CurrentQuests[i].CompletedSteps.Add(completedStep);

            // important
            GlobalSaveManager.QuestData quest = CurrentQuests[i];
            quest.IsComplete = CurrentQuests[i].CompletedSteps.Count == questResource.Steps.Length;
            CurrentQuests[i] = quest;
            EmitSignal(nameof(QuestUpdated));

            if (quest.IsComplete)
            {
                SortQuests(i);
                PlayerHUD.Instance.QueueNotification("Quest Complete!", title);
                DisperseRewards(questResource);
            }
            else
                PlayerHUD.Instance.QueueNotification("Quest Updated", $"{title}: {completedStep}");

            return;
        }

        quests.Add(questResource);

        GlobalSaveManager.QuestData questData = new GlobalSaveManager.QuestData
        {
            Title = title,
            IsComplete = false,
            CompletedSteps = new List<string>()
        };

        if (completedStep != "")
            questData.CompletedSteps.Add(completedStep);

        CurrentQuests.Add(questData);
        EmitSignal(nameof(QuestUpdated));

        PlayerHUD.Instance.QueueNotification("Quest Added", title);
    }

    private void DisperseRewards(QuestResource quest)
    {
        GlobalPlayerManager.Instance.Player.UpdateXP(quest.RewardXp);
        string message = $"{quest.RewardXp}xp";

        foreach (SlotData slotData in quest.RewardItems)
        {
            GlobalPlayerManager.Instance.PlayerInventory.AddItem(slotData.Item, slotData.Quantity);
            message += $", {slotData.Quantity}{(slotData.Quantity > 1 ? $"{slotData.Item.Name}s" : slotData.Item.Name)}";
        }

        PlayerHUD.Instance.QueueNotification("Quest Rewards Received!", message);
    }

    public GlobalSaveManager.QuestData FindQuest(QuestResource quest)
    {
        foreach (GlobalSaveManager.QuestData questData in CurrentQuests)
            if (questData.Title == quest.Title)
                return questData;

        return new GlobalSaveManager.QuestData
        {
            Title = "not found",
            IsComplete = false,
            CompletedSteps = new List<string>()
        };
    }

    public QuestResource FindQuestByTitle(string title)
    {
        foreach (QuestResource quest in quests)
            if (quest.Title == title)
                return quest;

        return null;
    }

    private int GetQuestIndexByTitle(string title)
    {
        for (int i = 0; i < CurrentQuests.Count; i++)
            if (CurrentQuests[i].Title == title)
                return i;

        return -1;
    }

    private void SortQuests(int index)
    {
        GlobalSaveManager.QuestData questData = CurrentQuests[index];
        CurrentQuests.RemoveAt(index);
        int newIndex = CurrentQuests.FindIndex(quest => quest.IsComplete);
        CurrentQuests.Insert(newIndex < 0 ? CurrentQuests.Count : newIndex, questData);
    }
}
