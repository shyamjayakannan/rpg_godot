using Godot;

public partial class Bomb : Throwable
{
    // Export
    [Export]
    private float fuseDuration = 4;
    [Export]
    private float bounciness = 2;
    [Export]
    private int maxBounces = 5;

    // private
    private int bounceCount = 0;
    private float throwDistance; // use local so that original is not modified if bomb is picked up again

    // methods
    public override void _Ready()
    {
        base._Ready();
        throwDistance = ThrowDistance;

        // VERY IMPORTANT
        // keep throwable below animation player so that the animation player can start te default animation before
        // queue is called. otherwise queue will be called before and then the animationplayer will play the default animation
        // later (in its _Ready) and the queue wll be overridden, causing no explosion! 
        AnimationPlayer.Queue("explode");
        AnimationPlayer.Connect(AnimationPlayer.SignalName.AnimationChanged, new(this, MethodName.OnAnimationChanged));
        AnimationPlayer.SpeedScale = (float)AnimationPlayer.CurrentAnimationLength / fuseDuration;
    }

    private void OnAnimationChanged(string _, string __)
    {
        AnimationPlayer.SpeedScale = 1;
        Shadow?.Hide();
    }

    protected override void OnCollision()
    {
        ThrowVelocity *= -1;
        ThrowDirection *= -1;
    }

    protected override void OnTimeout()
    {
        bounceCount++;

        if (bounceCount <= maxBounces)
        {
            throwDistance /= bounciness;
            SpeedAtTouchDown /= bounciness;
            ThrowSpeedWallDetect = -1 * SpeedAtTouchDown;
            Timer = 2 * SpeedAtTouchDown / Gravity;
            Vector2 landLocation = throwDistance * 32 * ThrowDirection;
            ThrowVelocity = new Vector2(landLocation.X, landLocation.Y - 0.5f * Gravity * Mathf.Pow(Timer, 2)) / Timer;
        }
        else
        {
            bounceCount = 0;
            throwDistance = ThrowDistance;
            SetPhysicsProcess(false);
            HurtBox.Monitorable = false;
            WallDetect.Monitoring = false;
            WallDetect.Monitorable = false;

            // so that it can be picked up again
            Connect(Area2D.SignalName.AreaEntered, new(this, Interactables.MethodName.OnArea2DAreaEntered));
            Connect(Area2D.SignalName.AreaExited, new(this, Interactables.MethodName.OnArea2DAreaExited));
        }
    }
}
