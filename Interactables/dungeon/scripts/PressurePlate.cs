using Godot;

public partial class PressurePlate : Node2D
{
    [Signal]
    private delegate void PressurePlateActivatedEventHandler();

    // private
    private bool isActive = false;
    private int bodies = 0;
    private Rect2 offRect;
    private Area2D area2D;
    private AudioStreamPlayer2D audioStreamPlayer2D;
    private Sprite2D sprite;
    private AudioStream audioActivate = GD.Load<AudioStream>("res://Interactables/dungeon/lever-01.wav");
    private AudioStream audioDeactivate = GD.Load<AudioStream>("res://Interactables/dungeon/lever-02.wav");
    private PersistentDataHandler persistentDataHandler;

    // methods
    public override void _Ready()
    {
        area2D = GetNode<Area2D>("Area2D");
        audioStreamPlayer2D = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        sprite = GetNode<Sprite2D>("Sprite2D");
        persistentDataHandler = GetNode<PersistentDataHandler>("PersistentDataHandler");

        offRect = sprite.RegionRect;

        persistentDataHandler.Connect(PersistentDataHandler.SignalName.DataLoaded, new(this, MethodName.SetState));
        persistentDataHandler.GetValue();

        area2D.Connect(Area2D.SignalName.BodyEntered, new(this, MethodName.OnArea2DBodyEntered));
        area2D.Connect(Area2D.SignalName.BodyExited, new(this, MethodName.OnArea2DBodyExited));
        GlobalSignalManager.Instance.Connect(GlobalSignalManager.SignalName.BarredDoorStateChanged, new(this, MethodName.SetValue));

        // before next level loads, do this so that pressure plate is not deactivated when the statue leaves
        // the scenetree and onarea2dbodyexited is called
        GlobalLevelManager.Instance.Connect(GlobalLevelManager.SignalName.LevelLoadStarted, Callable.From(() => isActive = false));
    }

    private void SetValue(bool value)
    {
        if (value)
            persistentDataHandler.SetValue();
        else
            persistentDataHandler.UnsetValue();
    }

    private void SetState(bool value)
    {
        if (!value)
            return;

        isActive = true;
        sprite.RegionRect = new Rect2(
            new Vector2(
                offRect.Position.X - 32,
                sprite.RegionRect.Position.Y
            ),
            sprite.RegionRect.Size
        );
    }

    private void OnArea2DBodyEntered(Node body)
    {
        bodies += 1;
        CheckIsActivated();
    }

    private void OnArea2DBodyExited(Node body)
    {
        bodies -= 1;
        CheckIsActivated();
    }

    private void CheckIsActivated()
    {
        if (bodies > 0 && !isActive)
        {
            isActive = true;
            sprite.RegionRect = new Rect2(
                new Vector2(
                    offRect.Position.X - 32,
                    sprite.RegionRect.Position.Y
                ),
                sprite.RegionRect.Size
            );
            PlayAudio(audioActivate);
            EmitSignal(nameof(PressurePlateActivated));
            GlobalSignalManager.Instance.EmitSignal(GlobalSignalManager.SignalName.PressurePlateActivated);
        }
        else if (bodies <= 0 && isActive)
        {
            isActive = false;
            sprite.RegionRect = new Rect2(
                new Vector2(
                    offRect.Position.X,
                    sprite.RegionRect.Position.Y
                ),
                sprite.RegionRect.Size
            );
            PlayAudio(audioDeactivate);
            GlobalSignalManager.Instance.EmitSignal(GlobalSignalManager.SignalName.PressurePlateDeactivated);
        }
    }

    private void PlayAudio(AudioStream stream)
    {
        audioStreamPlayer2D.Stream = stream;
        audioStreamPlayer2D.Play();
    }
}
