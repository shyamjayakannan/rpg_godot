using Godot;

public class QuestStepItem : Control
{
    // properties
    protected Label Label { get; set; }
    protected Sprite Sprite { get; set; }

    // methods
    public override void _Ready()
    {
        Label = GetNode<Label>("Label");
        Sprite = GetNode<Sprite>("Sprite");
    }

    public void Initialize(bool isComplete, string step)
    {
        Label.Text = step;
        Sprite.Frame = isComplete ? 1 : 0;
    }
}
