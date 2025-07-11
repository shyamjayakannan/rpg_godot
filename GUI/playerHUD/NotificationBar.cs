using System.Collections.Generic;
using Godot;

public partial class NotificationBar : Control
{
    // private
    private PanelContainer panelContainer;
    private AnimationPlayer animationPlayer;
    private Label title;
    private Label message;
    private Queue<Notification> notifications = new();

    // properties
    public new struct Notification
    {
        public string Message { get; set; }
        public string Type { get; set; }
    }

    // methods
    public override void _Ready()
    {
        panelContainer = GetNode<PanelContainer>("PanelContainer");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        title = GetNode<Label>("PanelContainer/VBoxContainer/Title");
        message = GetNode<Label>("PanelContainer/VBoxContainer/Message");
        panelContainer.Hide();

        animationPlayer.Connect(AnimationMixer.SignalName.AnimationFinished, new(this, MethodName.DisplayNotification));
    }

    private void DisplayNotification(string _ = null)
    {
        if (notifications == null || notifications.Count == 0)
            return;

        Notification notification = notifications.Dequeue();
        title.Text = notification.Type;
        message.Text = notification.Message;
        animationPlayer.Play("showNotification");
    }

    public void AddNotificationToQueue(string title, string message)
    {
        notifications.Enqueue(new()
        {
            Type = title,
            Message = message
        });

        if (animationPlayer.IsPlaying())
            return;

        DisplayNotification();
    }
}
