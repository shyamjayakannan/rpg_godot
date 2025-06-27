using Godot;

public class ButtonMenu : VBoxContainer
{
    // Exports
    [Export]
    public AudioStream buttonFocusSound;
    [Export]
    public AudioStream buttonPressSound;

    // methods
    public void PlayFocus(AudioStreamPlayer audioStreamPlayer)
    {
        audioStreamPlayer.Stream = buttonFocusSound;
        audioStreamPlayer.Play();
    }

    public void PlayPress(AudioStreamPlayer audioStreamPlayer)
    {
        audioStreamPlayer.Stream = buttonPressSound;
        audioStreamPlayer.Play();
    }

    public void ConnectFocus(Button button, AudioStreamPlayer audioStreamPlayer)
    {
        button.Connect("focus_entered", this, nameof(PlayFocus), new Godot.Collections.Array(audioStreamPlayer));
    }

    public void DisconnectFocus(Button button)
    {
        button.Disconnect("focus_entered", this, nameof(PlayFocus));
    }

    public bool IsConnectedFocus(Button button)
    {
        return button.IsConnected("focus_entered", this, nameof(PlayFocus));
    }
}
