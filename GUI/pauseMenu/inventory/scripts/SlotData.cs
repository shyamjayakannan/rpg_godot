using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(SlotData), "", nameof(Resource))]
public partial class SlotData : Resource
{
	// Exports
	[Export]
	public int Quantity { get; set; } = 0;
	[Export]
	public Items Item { get; private set; }

	// constructors
	public SlotData(int quantity, Items item)
	{
		Quantity = quantity;
		Item = item;
	}

	public SlotData()
	{

	}
}
