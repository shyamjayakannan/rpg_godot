using Godot;

public class ButtonMenu : VBoxContainer
{
    // properties
    public static AudioStream ButtonFocusSound { get; private set; } = GD.Load<AudioStream>("res://title_screen/menu_focus.wav");
    public static AudioStream ButtonPressSound { get; private set; } = GD.Load<AudioStream>("res://title_screen/menu_select.wav");

    // methods
    public void PlayFocus(AudioStreamPlayer audioStreamPlayer)
    {
        audioStreamPlayer.Stream = ButtonFocusSound;
        audioStreamPlayer.Play();
    }

    public void PlayPress(AudioStreamPlayer audioStreamPlayer)
    {
        audioStreamPlayer.Stream = ButtonPressSound;
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
