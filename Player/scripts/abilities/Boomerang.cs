using Godot;

public class Boomerang : Node2D
{
    // Exports
    [Export]
    private readonly float acceleration = 500.0f;
    [Export]
    private readonly float maxSpeed = 400.0f;

    // private
    private float speed = 0;
    private Vector2 direction;
    private AnimationPlayer animationPlayer;
    private AudioStreamPlayer2D audioStreamPlayer2D;
    private HurtBox hurtBox;
    private ItemMagnet itemMagnet;

    // public
    public enum State
    {
        INACTIVE,
        THROW,
        RETURN
    }
    public State BoomerangState { get; private set; } = State.INACTIVE;

    // methods
    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        audioStreamPlayer2D = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D2");
        hurtBox = GetNode<HurtBox>("HurtBox");
        itemMagnet = GetNode<ItemMagnet>("ItemMagnet");
        audioStreamPlayer2D.Stream = GD.Load<AudioStream>("res://Player/audio/catch.wav");

        Hide();
    }

    public override void _PhysicsProcess(float delta)
    {
        switch (BoomerangState)
        {
            case State.INACTIVE:
                return;

            case State.THROW:
                speed -= acceleration * delta;

                if (speed <= 0)
                    BoomerangState = State.RETURN;

                break;

            case State.RETURN:
                Player player = GlobalPlayerManager.Instance.Player;

                if (player.GlobalPosition.DistanceTo(GlobalPosition) <= 1)
                {
                    Hide();
                    BoomerangState = State.INACTIVE;
                    animationPlayer.Stop();
                    audioStreamPlayer2D.Play();
                    hurtBox.Monitorable = false;
                    itemMagnet.Monitoring = false;
                }

                speed += acceleration * delta;
                direction = GlobalPosition.DirectionTo(player.GlobalPosition);
                break;
        }

        Position += speed * direction * delta;
    }

    public void Throw(Vector2 throwDirection)
    {
        direction = throwDirection;
        speed = maxSpeed;
        animationPlayer.Play("boomerang");
        hurtBox.Monitorable = true;
        itemMagnet.Monitoring = true;
        BoomerangState = State.THROW;
        Show();
    }
}
