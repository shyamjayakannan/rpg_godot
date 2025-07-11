using Godot;

public partial class Arrow : Node2D
{
    // Exports
    [Export]
    private float moveSpeed = 300;
    [Export]
    private AudioStream audioStream;

    // private
    private Vector2 moveDirection = Vector2.Right;
    private HurtBox hurtBox;
    private Sprite2D sprite;
    private Sprite2D shadow;
    private AudioStreamPlayer2D audioStreamPlayer2D;

    // methods
    public override void _Ready()
    {
        sprite = GetNode<Sprite2D>("Sprite2D");
        shadow = GetNode<Sprite2D>("Shadow");
        hurtBox = GetNode<HurtBox>("HurtBox");
        audioStreamPlayer2D = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        audioStreamPlayer2D.Play();

        hurtBox.Connect(HurtBox.SignalName.DidDamage, new(this, Node.MethodName.QueueFree));
        GetTree().CreateTimer(10).Connect(SceneTreeTimer.SignalName.Timeout, new(this, Node.MethodName.QueueFree));
    }

    public override void _Process(double delta)
    {
        Position += moveSpeed * moveDirection * (float)delta;
    }

    public void Fire(Vector2 direction)
    {
        moveDirection = direction;
        float angle = moveDirection.Angle();
        sprite.Rotation = angle;
        shadow.Rotation = angle;
        hurtBox.Rotation = angle;
    }
}
