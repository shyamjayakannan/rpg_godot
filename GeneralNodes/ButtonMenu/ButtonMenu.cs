using Godot;

public partial class ButtonMenu : VBoxContainer
{
    // private
    Callable callable;

    // properties
    public static AudioStream ButtonFocusSound { get; private set; } = GD.Load<AudioStream>("res://title_screen/menu_focus.wav");
    public static AudioStream ButtonPressSound { get; private set; } = GD.Load<AudioStream>("res://title_screen/menu_select.wav");

    // methods
    public static void PlayFocus(AudioStreamPlayer audioStreamPlayer)
    {
        audioStreamPlayer.Stream = ButtonFocusSound;
        audioStreamPlayer.Play();
    }

    public static void PlayPress(AudioStreamPlayer audioStreamPlayer)
    {
        audioStreamPlayer.Stream = ButtonPressSound;
        audioStreamPlayer.Play();
    }

    public void ConnectFocus(Button button, AudioStreamPlayer audioStreamPlayer)
    {
        callable = Callable.From(() => PlayFocus(audioStreamPlayer));
        button.Connect(Control.SignalName.FocusEntered, callable);
    }

    public void DisconnectFocus(Button button)
    {
        button.Disconnect(Control.SignalName.FocusEntered, callable);
    }

    public bool IsConnectedFocus(Button button)
    {
        return button.IsConnected(Control.SignalName.FocusEntered, callable);
    }
}
