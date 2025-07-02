using Godot;

public class ItemDeliverQuestStepItem : QuestStepItem
{
    // private
    private TextureRect textureRect;

    // methods
    public override void _Ready()
    {
        Label = GetNode<Label>("HBoxContainer/Label");
        Sprite = GetNode<Sprite>("HBoxContainer/Sprite");
        textureRect = GetNode<TextureRect>("HBoxContainer/PanelContainer/TextureRect");
    }

    public void Initialize(bool isComplete, int stepCount, int totalSteps, Items item)
    {
        Label.Text = $"{stepCount}/{totalSteps} {item.Name}{(totalSteps > 1 ? "s" : "")}";
        Sprite.Frame = isComplete ? 1 : 0;
        textureRect.Texture = item.Texture;
    }
}
