using Godot;

public class Bomb : Throwable
{
    // Export
    [Export]
    private readonly float fuseDuration = 4;
    [Export]
    private readonly float bounciness = 2;
    [Export]
    private readonly int maxBounces = 5;

    // private
    private int bounceCount = 0;
    private float throwDistance; // use local so that original is not modified if bomb is picked up again

    // methods
    public override void _Ready()
    {
        base._Ready();
        throwDistance = ThrowDistance;
        AnimationPlayer.Queue("explode");
        AnimationPlayer.Connect("animation_changed", this, nameof(OnAnimationChanged));
        AnimationPlayer.PlaybackSpeed = AnimationPlayer.CurrentAnimationLength / fuseDuration;
    }

    private void OnAnimationChanged(string oldName, string newName)
    {
        AnimationPlayer.PlaybackSpeed = 1;
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
            Timer.WaitTime = 2 * SpeedAtTouchDown / Gravity;
            Vector2 landLocation = throwDistance * 32 * ThrowDirection;
            GD.Print(landLocation);
            ThrowVelocity = new Vector2(landLocation.x, landLocation.y - 0.5f * Gravity * Mathf.Pow(Timer.WaitTime, 2)) / Timer.WaitTime;
            Timer.Start();
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
            Connect("area_entered", this, nameof(OnArea2DAreaEntered));
            Connect("area_exited", this, nameof(OnArea2DAreaExited));
        }
    }
}
