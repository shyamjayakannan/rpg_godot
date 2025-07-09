using Godot;

[Tool]
[GlobalClass]
public partial class QuestResource : Resource
{
    // Exports
    [Export]
    public string Title { get; private set; }
    [Export(PropertyHint.MultilineText)]
    public string Description { get; private set; }
    [Export]
    public QuestStepResource[] Steps { get; private set; }
    [Export]
    public int RewardXp { get; private set; }
    [Export]
    public SlotData[] RewardItems { get; private set; } = System.Array.Empty<SlotData>();
}
