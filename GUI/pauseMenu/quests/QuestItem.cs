using Godot;

namespace Rpg
{
    public partial class QuestItem : Button
    {
        // private
        private Label title;
        private Label step;
        private Label titleEllipsis;
        private Label stepEllipsis;
        private Font font;

        // properties
        public QuestResource Quest { get; private set; }

        // methods
        public override void _Ready()
        {
            title = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/Title");
            titleEllipsis = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/Ellipsis");
            step = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer2/Step");
            stepEllipsis = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer2/Ellipsis");
            font = GetThemeFont("m5x7");
        }

        public void Initialize(QuestResource _quest, GlobalSaveManager.QuestData questData)
        {
            Quest = _quest;
            title.Text = Quest.Title;

            if (questData.IsComplete)
            {
                step.Text = "Completed";
                step.Modulate = Colors.LightGreen;
            }
            else
                step.Text = $"stage: {questData.CompletedSteps.Count}/{_quest.Steps.Length}";

            titleEllipsis.Visible = font.GetStringSize(title.Text).X > title.Size.X;
            stepEllipsis.Visible = font.GetStringSize(step.Text).X > step.Size.X;
        }
    }
}
