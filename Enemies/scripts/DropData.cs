using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(DropData), "", nameof(Resource))]
public class DropData : Resource
{
    // Exports
    [Export]
    public readonly Items item;
    [Export(PropertyHint.Range, "0, 100, 1")]
    private readonly float probability = 100;
    [Export(PropertyHint.Range, "0, 10, 1")]
    private readonly int minAmount = 1;
    [Export(PropertyHint.Range, "0, 10, 1")]
    private readonly int maxAmount = 1;

    // methods
    public int GetDropCount()
    {
        if (GD.RandRange(0, 100) >= probability)
            return 0;

        return (int)(GD.Randi() % (maxAmount - minAmount + 1) + minAmount);
    }
}
