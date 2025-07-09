using System.Linq;
using Godot;

public partial class QuestUI : Control
{
    // private
    private PackedScene questItemScene = GD.Load<PackedScene>("res://GUI/pauseMenu/quests/QuestItem.tscn");
    private PackedScene questStepItemScene = GD.Load<PackedScene>("res://GUI/pauseMenu/quests/QuestStepItem.tscn");
    private PackedScene itemDeliverQuestStepItemScene = GD.Load<PackedScene>("res://GUI/pauseMenu/quests/ItemDeliverQuestStepItem.tscn");
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

        Connect(CanvasItem.SignalName.VisibilityChanged, new(this, MethodName.OnVisibilityChanged));
    }

    private void OnVisibilityChanged()
    {
        foreach (QuestItem questItem in vBoxContainer.GetChildren().Cast<QuestItem>())
            questItem.QueueFree();

        if (!Visible)
            return;

        foreach (GlobalSaveManager.QuestData questData in GlobalQuestManager.Instance.CurrentQuests)
        {
            QuestResource questResource = GlobalQuestManager.Instance.FindQuestByTitle(questData.Title);

            if (questResource == null)
                continue;

            QuestItem questItem = (QuestItem)questItemScene.Instantiate();

            // VERY IMPORTANT
            // do addchild before initialize because initialize required onready variables
            vBoxContainer.AddChild(questItem);
            questItem.Initialize(questResource, questData);
            questItem.Connect(Control.SignalName.FocusEntered, Callable.From(() => OnFocusEntered(questItem.Quest)));
        }

        ClearDescription();
        GetTree().CreateTimer(0.1f).Connect(SceneTreeTimer.SignalName.Timeout, Callable.From(() => vBoxContainer.GetChildOrNull<QuestItem>(0)?.GrabFocus()));
    }

    private void ClearDescription()
    {
        title.Text = "";
        description.Text = "";

        foreach (QuestStepItem questStepItem in stepContainer.GetChildren().Cast<QuestStepItem>())
            questStepItem.QueueFree();
    }

    private void OnFocusEntered(QuestResource quest)
    {
        ClearDescription();
        ButtonMenu.PlayFocus(PauseMenu.Instance.AudioStreamPlayer);
        title.Text = quest.Title;
        description.Text = quest.Description;

        foreach (QuestStepItem questStepItem in stepContainer.GetChildren().Cast<QuestStepItem>())
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
                ItemDeliverQuestStepItem itemDeliverQuestStepItem = (ItemDeliverQuestStepItem)itemDeliverQuestStepItemScene.Instantiate();
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
                QuestStepItem questStepItem = (QuestStepItem)questStepItemScene.Instantiate();
                stepContainer.AddChild(questStepItem);
                questStepItem.Initialize(questData.CompletedSteps.Contains(questStep.Step), questStep.Step);
            }
        }
    }
}
