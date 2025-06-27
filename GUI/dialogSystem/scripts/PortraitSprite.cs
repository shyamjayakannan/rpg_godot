using Godot;

// so that dialogsystem can recognize the cast (line 58). this is needed because dialogsystem is a tool script
// and when it runs in the editor, unless custom types are also tool scripts, it wont recognize them
[Tool]
public class PortraitSprite : Sprite
{
    // private
    private bool blink = false;
    private bool openMouth = false;
    private int mouthOpenFrames = 0;
    private AudioStreamPlayer audioStreamPlayer;

    // properties
    private bool Blink
    {
        get => blink;
        set => SetBlink(value);
    }
    private bool OpenMouth
    {
        get => openMouth;
        set => SetOpenMouth(value);
    }
    public float BasePitch { get; set; }

    // methods
    public override void _Ready()
    {
        audioStreamPlayer = GetNode<AudioStreamPlayer>("../AudioStreamPlayer");

        if (Engine.EditorHint)
            return;

        Blink = false;
    }

    public void OnLetterAdded(string letter)
    {
        if ("aeiou1234567890".Contains(letter))
        {
            OpenMouth = true;
            mouthOpenFrames += 3;
            audioStreamPlayer.PitchScale = (float)GD.RandRange(BasePitch - 0.05, BasePitch + 0.05);
            audioStreamPlayer.Play();
        }
        else if (".,?!".Contains(letter))
            mouthOpenFrames = 0;

        if (mouthOpenFrames > 0)
            mouthOpenFrames--;

        if (mouthOpenFrames == 0)
            OpenMouth = false;
    }

    private void SetOpenMouth(bool value)
    {
        if (OpenMouth == value)
            return;

        openMouth = value;
        UpdatePortrait();
    }

    private void SetBlink(bool value)
    {
        GetTree().CreateTimer(Blink ? 0.15f : (float)GD.RandRange(0.1, 3)).Connect("timeout", this, nameof(SetBlink2), new Godot.Collections.Array(value));
    }

    private void SetBlink2(bool value)
    {
        blink = value;
        UpdatePortrait();

        // recursion
        Blink = !Blink;
    }


    private void UpdatePortrait()
    {
        Frame = openMouth ? 2 : 0;

        if (Blink)
            Frame += 1;
    }
}
