using Godot;

namespace Rpg
{
    public partial class ItemDeliverQuestStepItem : QuestStepItem
    {
        // private
        private TextureRect textureRect;

        // methods
        public override void _Ready()
        {
            Label = GetNode<Label>("HBoxContainer/Label");
            Sprite2D = GetNode<Sprite2D>("HBoxContainer/Sprite2D");
            textureRect = GetNode<TextureRect>("HBoxContainer/PanelContainer/TextureRect");
        }

        public void Initialize(bool isComplete, int stepCount, int totalSteps, Items item)
        {
            Label.Text = $"{stepCount}/{totalSteps} {item.Name}{(totalSteps > 1 ? "s" : "")}";
            Sprite2D.Frame = isComplete ? 1 : 0;
            textureRect.Texture = item.Texture2D;
        }
    }
}
