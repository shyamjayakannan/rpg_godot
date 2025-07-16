using Godot;

namespace Rpg
{
    [GlobalClass]
    public partial class DropData : Resource
    {
        // Exports
        [Export]
        public Items item;
        [Export(PropertyHint.Range, "0, 100, 1")]
        private float probability = 100;
        [Export(PropertyHint.Range, "0, 10, 1")]
        private int minAmount = 1;
        [Export(PropertyHint.Range, "0, 10, 1")]
        private int maxAmount = 1;

        // methods
        public int GetDropCount()
        {
            if (GD.RandRange(0, 100) >= probability)
                return 0;

            return (int)(GD.Randi() % (maxAmount - minAmount + 1) + minAmount);
        }
    }
}
