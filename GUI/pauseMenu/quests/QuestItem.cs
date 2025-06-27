using Godot;

public class QuestItem : Button
{
    // private
    private Label title;
    private Label step;
    private Label titleEllipsis;
    private Label stepEllipsis;

    // properties
    public QuestResource Quest { get; private set; }

    // methods
    public override void _Ready()
    {
        title = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/Title");
        titleEllipsis = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/Ellipsis");
        step = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer2/Step");
        stepEllipsis = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer2/Ellipsis");
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

        titleEllipsis.Visible = IsLabelClipped(title);
        stepEllipsis.Visible = IsLabelClipped(step);
    }

    private bool IsLabelClipped(Label label)
    {
        var font = label.GetFont("m5x7");

        if (font == null)
            return false;

        return font.GetStringSize(label.Text).x > label.RectSize.x;
    }
}
