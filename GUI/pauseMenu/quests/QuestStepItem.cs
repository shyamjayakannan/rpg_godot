using Godot;

public partial class QuestStepItem : Control
{
    // properties
    protected Label Label { get; set; }
    protected Sprite2D Sprite2D { get; set; }

    // methods
    public override void _Ready()
    {
        Label = GetNode<Label>("Label");
        Sprite2D = GetNode<Sprite2D>("Sprite2D");
    }

    public void Initialize(bool isComplete, string step)
    {
        Label.Text = step;
        Sprite2D.Frame = isComplete ? 1 : 0;
    }
}
