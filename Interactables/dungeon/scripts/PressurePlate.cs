using Godot;

public class PressurePlate : Node2D
{
    [Signal]
    private delegate void PressurePlateActivated();

    // private
    private bool isActive = false;
    private int bodies = 0;
    private Rect2 offRect;
    private Area2D area2D;
    private AudioStreamPlayer2D audioStreamPlayer2D;
    private Sprite sprite;
    private readonly AudioStream audioActivate = GD.Load<AudioStream>("res://Interactables/dungeon/lever-01.wav");
    private readonly AudioStream audioDeactivate = GD.Load<AudioStream>("res://Interactables/dungeon/lever-02.wav");
    private PersistentDataHandler persistentDataHandler;

    // methods
    public override void _Ready()
    {
        area2D = GetNode<Area2D>("Area2D");
        audioStreamPlayer2D = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        sprite = GetNode<Sprite>("Sprite");
        persistentDataHandler = GetNode<PersistentDataHandler>("PersistentDataHandler");

        offRect = sprite.RegionRect;

        persistentDataHandler.Connect(nameof(PersistentDataHandler.DataLoaded), this, nameof(SetState));
        persistentDataHandler.GetValue();

        area2D.Connect("body_entered", this, nameof(OnArea2DBodyEntered));
        area2D.Connect("body_exited", this, nameof(OnArea2DBodyExited));
        GlobalSignalManager.Instance.Connect(nameof(GlobalSignalManager.BarredDoorStateChanged), this, nameof(SetValue));
        GlobalLevelManager.Instance.Connect(nameof(GlobalLevelManager.LevelLoadStarted), this, nameof(SetIsActive));
    }

    private void SetIsActive()
    {
        // before next level loads, do this so that pressure plate is not deactivated when the statue leaves
        // the scenetree and onarea2dbodyexited is called
        isActive = false;
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
                offRect.Position.x - 32,
                sprite.RegionRect.Position.y
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
                    offRect.Position.x - 32,
                    sprite.RegionRect.Position.y
                ),
                sprite.RegionRect.Size
            );
            PlayAudio(audioActivate);
            EmitSignal(nameof(PressurePlateActivated));
            GlobalSignalManager.Instance.EmitSignal(nameof(GlobalSignalManager.PressurePlateActivated));
        }
        else if (bodies <= 0 && isActive)
        {
            isActive = false;
            sprite.RegionRect = new Rect2(
                new Vector2(
                    offRect.Position.x,
                    sprite.RegionRect.Position.y
                ),
                sprite.RegionRect.Size
            );
            PlayAudio(audioDeactivate);
            GlobalSignalManager.Instance.EmitSignal(nameof(GlobalSignalManager.PressurePlateDeactivated));
        }
    }

    private void PlayAudio(AudioStream stream)
    {
        audioStreamPlayer2D.Stream = stream;
        audioStreamPlayer2D.Play();
    }
}
