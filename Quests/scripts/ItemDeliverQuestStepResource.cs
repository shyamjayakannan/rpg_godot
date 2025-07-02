using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(QuestStepResource), "", nameof(Resource))]
public class ItemDeliverQuestStepResource : QuestStepResource
{
    // Exports
    [Export]
    public Items Item
    {
        get => item;
        private set
        {
            item = value;
            SetString();
        }
    }
    [Export]
    public int Quantity
    {
        get => quantity;
        private set
        {
            quantity = value;
            SetString();
        }
    }

    // private
    private Items item;
    private int quantity;

    // methods
    private void SetString()
    {
        Step = $"deliver {Quantity} {Item?.Name}{(Quantity > 1 ? "s" : "")}";
        PropertyListChangedNotify();
    }
}