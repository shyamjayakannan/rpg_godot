using Godot;

namespace Rpg
{
    [Tool]
    [GlobalClass]
    public partial class QuestStepResource : Resource
    {
        // Exports
        [Export]
        public string Step { get; set; }
    }
}
