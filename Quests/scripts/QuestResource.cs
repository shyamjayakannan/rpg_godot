using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(QuestResource), "", nameof(Resource))]
public class QuestResource : Resource
{
    // Exports
    [Export]
    public string Title { get; private set; }
    [Export(PropertyHint.MultilineText)]
    public string Description { get; private set; }
    [Export]
    public string[] Steps { get; private set; }
    [Export]
    public int RewardXp { get; private set; }
    [Export]
    public SlotData[] RewardItems { get; private set; } = new SlotData[0];
}
