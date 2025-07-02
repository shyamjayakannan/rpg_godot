using Godot;

public class QuestUI : Control
{
    // private
    private readonly PackedScene questItemScene = GD.Load<PackedScene>("res://GUI/pauseMenu/quests/QuestItem.tscn");
    private readonly PackedScene questStepItemScene = GD.Load<PackedScene>("res://GUI/pauseMenu/quests/QuestStepItem.tscn");
    private readonly PackedScene itemDeliverQuestStepItemScene = GD.Load<PackedScene>("res://GUI/pauseMenu/quests/ItemDeliverQuestStepItem.tscn");
    private ButtonMenu vBoxContainer;
    private VBoxContainer stepContainer;
    private Label title;
    private Label description;

    // methods
    public override void _Ready()
    {
        vBoxContainer = GetNode<ButtonMenu>("ScrollContainer/ButtonMenu");
        title = GetNode<Label>("VBoxContainer/Title");
        description = GetNode<Label>("VBoxContainer/Description");
        stepContainer = GetNode<VBoxContainer>("VBoxContainer/ScrollContainer/VBoxContainer");

        Connect("visibility_changed", this, nameof(OnVisibilityChanged));
    }

    private void OnVisibilityChanged()
    {
        foreach (QuestItem questItem in vBoxContainer.GetChildren())
            questItem.QueueFree();

        if (!Visible)
            return;

        foreach (GlobalSaveManager.QuestData questData in GlobalQuestManager.Instance.CurrentQuests)
        {
            QuestResource questResource = GlobalQuestManager.Instance.FindQuestByTitle(questData.Title);

            if (questResource == null)
                continue;

            QuestItem questItem = (QuestItem)questItemScene.Instance();

            // VERY IMPORTANT
            // do addchild before initialize because initialize required onready variables
            vBoxContainer.AddChild(questItem);
            questItem.Initialize(questResource, questData);
            questItem.Connect("focus_entered", this, nameof(OnFocusEntered), new Godot.Collections.Array(questItem.Quest));
        }

        ClearDescription();
        GetTree().CreateTimer(0.1f).Connect("timeout", this, nameof(OnTimerTimeout));
    }

    private void OnTimerTimeout()
    {
        vBoxContainer.GetChildOrNull<QuestItem>(0)?.GrabFocus();
    }

    private void ClearDescription()
    {
        title.Text = "";
        description.Text = "";

        foreach (QuestStepItem questStepItem in stepContainer.GetChildren())
            questStepItem.QueueFree();
    }

    private void OnFocusEntered(QuestResource quest)
    {
        ClearDescription();
        vBoxContainer.PlayFocus(PauseMenu.Instance.AudioStreamPlayer);
        title.Text = quest.Title;
        description.Text = quest.Description;

        foreach (QuestStepItem questStepItem in stepContainer.GetChildren())
            questStepItem.QueueFree();

        GlobalSaveManager.QuestData questData = GlobalQuestManager.Instance.FindQuest(quest);

        if (questData.Title == "not found")
            return;

        foreach (QuestStepResource questStep in quest.Steps)
        {
            // VERY IMPORTANT
            // do addchild before initialize because initialize required onready variables
            if (questStep is ItemDeliverQuestStepResource itemDeliverQuestStepResource)
            {
                ItemDeliverQuestStepItem itemDeliverQuestStepItem = (ItemDeliverQuestStepItem)itemDeliverQuestStepItemScene.Instance();
                stepContainer.AddChild(itemDeliverQuestStepItem);
                bool isComplete = questData.CompletedSteps.Contains(itemDeliverQuestStepResource.Step);
                int stepCount;

                if (isComplete)
                    stepCount = itemDeliverQuestStepResource.Quantity;
                else
                    stepCount = questData.InCompleteSteps.Find(tuple => tuple.Item1 == itemDeliverQuestStepResource.Step).Item2;

                itemDeliverQuestStepItem.Initialize(isComplete, stepCount, itemDeliverQuestStepResource.Quantity, itemDeliverQuestStepResource.Item);
            }
            else
            {
                QuestStepItem questStepItem = (QuestStepItem)questStepItemScene.Instance();
                stepContainer.AddChild(questStepItem);
                questStepItem.Initialize(questData.CompletedSteps.Contains(questStep.Step), questStep.Step);
            }
        }
    }
}
