using Godot;

public class HealEffect : ItemEffects
{
    // Exports
    [Export]
    private readonly int healAmmount = 1;
    [Export]
    private readonly AudioStream audioStream;

    // methods
    public override void Use()
    {
        GlobalPlayerManager.Instance.Player.UpdateHP(healAmmount);
        PauseMenu.Instance.PlayAudio(audioStream);
    }
}
