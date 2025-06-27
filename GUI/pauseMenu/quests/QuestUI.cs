using Godot;

public class QuestUI : Control
{
    // private
    private readonly PackedScene questItemScene = GD.Load<PackedScene>("res://GUI/pauseMenu/quests/QuestItem.tscn");
    private readonly PackedScene questStepItemScene = GD.Load<PackedScene>("res://GUI/pauseMenu/quests/QuestStepItem.tscn");
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
        stepContainer = GetNode<VBoxContainer>("VBoxContainer/VBoxContainer");

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

        foreach (string questStep in quest.Steps)
        {
            QuestStepItem questStepItem = (QuestStepItem)questStepItemScene.Instance();

            // VERY IMPORTANT
            // do addchild before initialize because initialize required onready variables
            stepContainer.AddChild(questStepItem);
            questStepItem.Initialize(questStep, questData.CompletedSteps.Contains(questStep));
        }
    }

}
