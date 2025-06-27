using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(QuestActivatedSwitch), "res://Quests/utilityNodes/icons/quest_switch.png", nameof(Node2D))]
public class QuestActivatedSwitch : QuestNode
{
    // Signals
    [Signal]
    private delegate void IsActivatedChanged(bool value);

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
    [Export]
    private readonly bool reactToGlobalSignal = false;
    [Export]
    private readonly bool freeOnRemove = false;

    // private
    private CheckType checkType = CheckType.HasQuest;
    private bool IsActivated = false;
    private enum CheckType
    {
        HasQuest,
        QuestStepComplete,
        OnCurrentQuestStep,
        QuestComplete
    }

    // methods
    public override void _Ready()
    {
        if (Engine.EditorHint)
            return;

        GetNode<Sprite>("Sprite").QueueFree();

        if (reactToGlobalSignal)
        {
            GlobalQuestManager.Instance.Connect(nameof(GlobalQuestManager.QuestUpdated), this, nameof(OnQuestUpdated));
            GlobalSaveManager.Instance.Connect(nameof(GlobalSaveManager.GameLoaded), this, nameof(OnQuestUpdated));
        }

        CheckIsActivated();
    }

    private void OnQuestUpdated()
    {
        CheckIsActivated();
    }

    private void CheckIsActivated()
    {
        GlobalSaveManager.QuestData questData = GlobalQuestManager.Instance.FindQuest(LinkedQuest);

        if (questData.Title != "not found")
        {
            switch (CheckTypeInstance)
            {
                case CheckType.HasQuest:
                    SetIsActivated(true);
                    break;

                case CheckType.QuestComplete:
                    SetIsActivated(questData.IsComplete);
                    break;

                case CheckType.QuestStepComplete:
                    SetIsActivated(QuestStep > 0 && questData.CompletedSteps.Contains(GetStep()));
                    break;

                case CheckType.OnCurrentQuestStep:
                    string step = GetStep();

                    if (step == "N/A")
                        SetIsActivated(false);
                    else
                    {
                        if (questData.CompletedSteps.Contains(step))
                            SetIsActivated(false);
                        else
                        {
                            string previousStep = QuestStep <= LinkedQuest.Steps.Length && QuestStep > 1 ? LinkedQuest.Steps[QuestStep - 2] : "N/A";
                            SetIsActivated(previousStep == "N/A" || questData.CompletedSteps.Contains(previousStep));
                        }
                    }
                    break;
            }
        }
        else
            SetIsActivated(false);
    }

    private void SetIsActivated(bool value)
    {
        IsActivated = value;
        EmitSignal(nameof(IsActivatedChanged), value);

        if (IsActivated)
        {
            if (removeWhenActivated)
                HideChildren();
            else
                ShowChildren();
        }
        else
        {
            if (removeWhenActivated)
                ShowChildren();
            else
                HideChildren();
        }
    }

    private void ShowChildren()
    {
        foreach (Node2D child in GetChildren())
        {
            child.Show();
            child.SetProcess(true);
            child.SetPhysicsProcess(true);
            CallDeferred(nameof(SetCollisionBodies), child, true);
        }
    }

    private void HideChildren()
    {
        foreach (Node2D child in GetChildren())
        {
            child.CallDeferred("hide");
            child.CallDeferred("set_process", false);
            child.CallDeferred("set_physics_process", false);
            CallDeferred(nameof(SetCollisionBodies), child, false);

            if (freeOnRemove)
                child.QueueFree();
        }
    }

    private void SetCollisionBodies(Node parent, bool value)
    {
        Godot.Collections.Array children = parent.GetChildren();

        foreach (Node c in children)
        {
            if (c is CollisionShape2D collisionShape2D)
                collisionShape2D.Disabled = !value;

            SetCollisionBodies(c, value);
        }
    }

    protected override void UpdateSummary()
    {
        SettingsSummary = $"UPDATE QUEST\nQuest: {LinkedQuest.Title}\n";

        switch (CheckTypeInstance)
        {
            case CheckType.HasQuest:
                SettingsSummary += "Checking whether player has quest";
                break;

            case CheckType.QuestStepComplete:
                SettingsSummary += $"Checking whether player has completed step: {GetStep()}";
                break;

            case CheckType.OnCurrentQuestStep:
                SettingsSummary += $"Checking whether player is on step: {GetStep()}";
                break;

            case CheckType.QuestComplete:
                SettingsSummary += "Checking whether quest is complete";
                break;
        }

        // needed
        PropertyListChangedNotify();
    }
}
