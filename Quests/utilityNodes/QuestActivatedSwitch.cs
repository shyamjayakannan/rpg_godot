using System.Linq;
using Godot;

namespace Rpg
{
    [Tool]
    [GlobalClass, Icon("res://Quests/utilityNodes/icons/quest_switch.png")]
    public partial class QuestActivatedSwitch : QuestNode
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
        private bool removeWhenActivated = false;
        [Export]
        private bool reactToGlobalSignal = false;
        [Export]
        private bool freeOnRemove = false;

        // private
        private CheckType checkType = CheckType.HasQuest;
        private bool isActivated = false;
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
            if (Engine.IsEditorHint())
                return;

            GetNode<Sprite2D>("Sprite2D").QueueFree();

            if (reactToGlobalSignal)
            {
                GlobalQuestManager.Instance.Connect(GlobalQuestManager.SignalName.QuestUpdated, new(this, MethodName.OnQuestUpdated));
                GlobalSaveManager.Instance.Connect(GlobalSaveManager.SignalName.GameLoaded, new(this, MethodName.OnQuestUpdated));
            }

            CheckIsActivated();
        }

        private void OnQuestUpdated(string title, bool isStarted)
        {
            CheckIsActivated();
        }

        private void CheckIsActivated()
        {
            GlobalSaveManager.QuestData questData = GlobalQuestManager.Instance.FindQuest(LinkedQuest);

            if (questData.Title == "not found")
            {
                SetIsActivated(false);
                return;
            }

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

                    if (step == "N/A" || questData.CompletedSteps.Contains(step))
                    {
                        SetIsActivated(false);
                        return;
                    }

                    string previousStep = QuestStep <= LinkedQuest.Steps.Length && QuestStep > 1 ? LinkedQuest.Steps[QuestStep - 2].Step : "N/A";
                    SetIsActivated(previousStep == "N/A" || questData.CompletedSteps.Contains(previousStep));
                    break;
            }
        }

        private void SetIsActivated(bool value)
        {
            isActivated = value;

            if (isActivated)
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
            foreach (Node2D child in GetChildren().Cast<Node2D>())
            {
                child.Show();
                child.SetProcess(true);
                child.SetPhysicsProcess(true);
                CallDeferred(MethodName.SetCollisionBodies, child, true);
            }
        }

        private void HideChildren()
        {
            foreach (Node2D child in GetChildren().Cast<Node2D>())
            {
                child.CallDeferred(CanvasItem.MethodName.Hide);
                child.CallDeferred(Node.MethodName.SetProcess, false);
                child.CallDeferred(Node.MethodName.SetPhysicsProcess, false);
                CallDeferred(MethodName.SetCollisionBodies, child, false);

                if (freeOnRemove)
                    child.QueueFree();
            }
        }

        private static void SetCollisionBodies(Node parent, bool value)
        {
            Godot.Collections.Array<Node> children = parent.GetChildren();

            foreach (Node c in children.Select(v => (Node)v))
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
            NotifyPropertyListChanged();
        }
    }
}
