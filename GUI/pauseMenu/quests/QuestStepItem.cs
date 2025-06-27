using Godot;

public class QuestStepItem : Control
{
    // private
    private Label label;
    private Sprite sprite;

    // methods
    public override void _Ready()
    {
        label = GetNode<Label>("Label");
        sprite = GetNode<Sprite>("Sprite");
    }

    public void Initialize(string step, bool isComplete)
    {
        label.Text = step;
        sprite.Frame = isComplete ? 1 : 0;
    }
}
