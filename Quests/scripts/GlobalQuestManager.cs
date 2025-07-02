using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;

public class GlobalQuestManager : Node
{
    // Signals
    [Signal]
    public delegate void QuestUpdated();

    // private
    private readonly List<QuestResource> quests = new List<QuestResource>();
    private const string QUEST_LOCATION = "res://Quests/quests";
    private Dictionary<string, string> questTitleToFile = new Dictionary<string, string>();

    // properties
    public static GlobalQuestManager Instance { get; private set; }
    public List<GlobalSaveManager.QuestData> CurrentQuests { get; set; } = new List<GlobalSaveManager.QuestData>();

    // methods
    public override void _Ready()
    {
        Instance = this;
    }

    private void LoadQuestMapping()
    {
        File file = new File();
        file.Open($"{QUEST_LOCATION}/questMap.json", File.ModeFlags.Read);
        string json = file.GetAsText();
        file.Close();
        questTitleToFile = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
    }

    public void LoadQuests()
    {
        LoadQuestMapping();
        quests.Clear();

        foreach (GlobalSaveManager.QuestData questData in CurrentQuests)
        {
            if (questTitleToFile.TryGetValue(questData.Title, out string fileName))
            {
                QuestResource questResource = GD.Load<QuestResource>($"{QUEST_LOCATION}/{fileName}");

                if (questResource != null)
                    quests.Add(questResource);
            }
        }
    }

    public void UpdateQuest(string title, QuestResource questResource, string completedStep = "")
    {
        int i = GetQuestIndexByTitle(title);

        if (i > -1)
        {
            if (completedStep != "" && !CurrentQuests[i].CompletedSteps.Contains(completedStep))
                CurrentQuests[i].CompletedSteps.Add(completedStep);

            CheckComplete(title, questResource, i, completedStep);
            return;
        }

        AddQuest(title, questResource);
    }

    public void UpdateItemDeliverSteps(string title, QuestResource questResource, int inInventory, ItemDeliverQuestStepResource itemDeliverQuestStepResource)
    {
        int i = GetQuestIndexByTitle(title);

        if (i > -1)
        {
            int index = CurrentQuests[i].InCompleteSteps.FindIndex(stepTuple => stepTuple.Item1 == itemDeliverQuestStepResource.Step);

            if (index < 0)
            {
                CurrentQuests[i].InCompleteSteps.Add((itemDeliverQuestStepResource.Step, 0));
                index = CurrentQuests[i].InCompleteSteps.Count - 1;
            }

            // important
            (string step, int quantity) = CurrentQuests[i].InCompleteSteps[index];

            if (inInventory >= itemDeliverQuestStepResource.Quantity - quantity)
            {
                GlobalPlayerManager.Instance.PlayerInventory.RemoveItem(itemDeliverQuestStepResource.Item, itemDeliverQuestStepResource.Quantity - quantity);
                CurrentQuests[i].CompletedSteps.Add(step);
                CurrentQuests[i].InCompleteSteps.RemoveAt(index);
            }
            else
            {
                GlobalPlayerManager.Instance.PlayerInventory.RemoveItem(itemDeliverQuestStepResource.Item, inInventory);
                quantity += inInventory;
                CurrentQuests[i].InCompleteSteps[index] = (step, quantity);
            }

            CheckComplete(title, questResource, i, itemDeliverQuestStepResource.Step);
            return;
        }

        AddQuest(title, questResource);
    }

    private void CheckComplete(string title, QuestResource questResource, int i, string step)
    {
        // important
        GlobalSaveManager.QuestData quest = CurrentQuests[i];
        quest.IsComplete = questResource.Steps.Length == quest.CompletedSteps.Count;
        CurrentQuests[i] = quest;
        EmitSignal(nameof(QuestUpdated));
        PlayerHUD.Instance.QueueNotification("Quest Updated", $"{title}: {step}");

        if (quest.IsComplete)
        {
            SortQuests(i);
            PlayerHUD.Instance.QueueNotification("Quest Complete!", title);
            DisperseRewards(questResource);
        }
    }

    private void AddQuest(string title, QuestResource questResource)
    {
        quests.Add(questResource);

        GlobalSaveManager.QuestData questData = new GlobalSaveManager.QuestData
        {
            Title = title,
            IsComplete = false,
            CompletedSteps = new List<string>(),
            InCompleteSteps = new List<(string, int)>()
        };

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
