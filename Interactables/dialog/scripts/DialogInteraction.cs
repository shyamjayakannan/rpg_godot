using System.Collections.Generic;
using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(DialogInteraction), "res://GUI/dialogSystem/icons/chat_bubbles.png", nameof(Node2D))]
public class DialogInteraction : Intercatables
{
    // Signals
    [Signal]
    public delegate void PlayerInteracted();
    [Signal]
    public delegate void Finished();

    // Exports
    [Export]
    private bool enabled = true;

    // private
    private AnimationPlayer animationPlayer;
    private List<DialogItem> dialogItems = new List<DialogItem>();
    private Area2D area2D;

    // methods
    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        area2D = GetNode<Area2D>("Area2D");

        if (Engine.EditorHint)
            return;

        area2D.Connect("area_entered", this, nameof(OnArea2DAreaEntered));
        area2D.Connect("area_exited", this, nameof(OnArea2DAreaExited));

        foreach (Node child in GetChildren())
            if (child is DialogItem dialogItem)
                dialogItems.Add(dialogItem);
    }

    public override string _GetConfigurationWarning()
    {
        bool atLeastOneValidChild = false;

        foreach (Node child in GetChildren())
            if (child is DialogItem)
                atLeastOneValidChild = true;

        return atLeastOneValidChild ? "" : "please add at least one DialogItem as child";
    }

    protected override void OnInteractPressed()
    {
        EmitSignal(nameof(PlayerInteracted));

        async void Wait()
        {
            // need to wait for two idle frames so that animation plays and npc faces player
            await ToSignal(GetTree(), "idle_frame");
            await ToSignal(GetTree(), "idle_frame");

            DialogSystem.Instance.ShowDialog(dialogItems);
            DialogSystem.Instance.Connect(nameof(DialogSystem.Finished), this, nameof(OnFinished));
        }

        Wait();
    }

    protected override void OnArea2DAreaEntered(Area2D area)
    {
        if (!enabled || dialogItems.Count == 0)
            return;

        base.OnArea2DAreaEntered(area);
        animationPlayer.Play("show");
    }

    protected override void OnArea2DAreaExited(Area2D area)
    {
        if (!enabled || dialogItems.Count == 0)
            return;

        base.OnArea2DAreaExited(area);
        animationPlayer.Play("hide");
    }

    private void OnFinished()
    {
        DialogSystem.Instance.Disconnect(nameof(DialogSystem.Finished), this, nameof(OnFinished));
        EmitSignal(nameof(Finished));
    }
}
