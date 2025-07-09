using Godot;

public partial class HealEffect : ItemEffects
{
    // Exports
    [Export]
    private int healAmmount = 1;
    [Export]
    private AudioStream audioStream;

    // methods
    public override void Use()
    {
        GlobalPlayerManager.Instance.Player.UpdateHP(healAmmount);
        PauseMenu.Instance.PlayAudio(audioStream);
    }
}
