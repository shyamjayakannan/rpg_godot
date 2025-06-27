using System;
using Godot;

public class ShopMenu : CanvasLayer
{
    // Signals
    [Signal]
    private delegate void Shown();
    [Signal]
    private delegate void Hidden();

    // private
    private Button closeButton;
    private AudioStreamPlayer audioStreamPlayer;
    private AudioStream openShopAudio = GD.Load<AudioStream>("res://GUI/shopMenu/audio/open_shop.wav");
    private AudioStream purchaseAudio = GD.Load<AudioStream>("res://GUI/shopMenu/audio/purchase.wav");
    private AudioStream errorAudio = GD.Load<AudioStream>("res://GUI/shopMenu/audio/error.wav");

    // methods
    public override void _Ready()
    {
        closeButton = GetNode<Button>("Control/Close");
        audioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

        closeButton.Connect("pressed", this, nameof(SetMenu), new Godot.Collections.Array(false));
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible || !@event.IsActionPressed("ui_cancel"))
            return;

        // dont let pausemenu be called when closing using esc key
        GetTree().SetInputAsHandled();
        SetMenu(false);
    }

    public void SetMenu(bool value)
    {
        if (value)
            PlayAudio(openShopAudio);

        Visible = value;
        EmitSignal(value ? nameof(Shown) : nameof(Hidden));
    }

    private void PlayAudio(AudioStream audioStream)
    {
        audioStreamPlayer.Stream = audioStream;
        audioStreamPlayer.Play();
    }
}
