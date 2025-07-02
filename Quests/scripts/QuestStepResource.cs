using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(QuestStepResource), "", nameof(Resource))]
public class QuestStepResource : Resource
{
    // Exports
    [Export]
    public string Step { get; set; }
}