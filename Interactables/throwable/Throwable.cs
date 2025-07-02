using Godot;

public class Throwable : Interactables
{
    // Exports
    [Export]
    protected float Gravity { get; private set; } = 980;
    [Export]
    protected float ThrowDistance { get; private set; } = 2; // in terms no. of of tiles

    // protected
    protected Area2D WallDetect { get; private set; }
    protected float ThrowSpeedWallDetect { get; set; } = 0;
    protected float SpeedAtTouchDown { get; set; }
    protected Timer Timer { get; private set; }
    protected Node2D ThrowableParent { get; private set; }
    protected AnimationPlayer AnimationPlayer { get; private set; }
    protected Vector2 ThrowVelocity { get; set; }
    protected Vector2 ThrowDirection { get; set; }
    protected HurtBox HurtBox { get; private set; }

    // methods
    public override void _Ready()
    {
        HurtBox = GetNode<HurtBox>("HurtBox");
        WallDetect = GetNode<Area2D>("WallDetect");
        Timer = GetNode<Timer>("Timer");
        ThrowableParent = (Node2D)GetParent();
        AnimationPlayer = ThrowableParent.GetNode<AnimationPlayer>("AnimationPlayer");
        SetupAreas();
        SetPhysicsProcess(false);

        Timer.Connect("timeout", this, nameof(OnTimeout));
        AnimationPlayer.Connect("animation_finished", this, nameof(OnAnimationPlayerAnimationFinished));
        Connect("area_entered", this, nameof(OnArea2DAreaEntered));
        Connect("area_exited", this, nameof(OnArea2DAreaExited));
    }

    public override void OnInteractPressed()
    {
        if (GlobalPlayerManager.Instance.Player.Throwable != null)
            return;

        SetCollisionBodies(ThrowableParent, true);
        ThrowableParent.GetParent()?.RemoveChild(ThrowableParent);
        GlobalPlayerManager.Instance.Player.PickupItem(this, ThrowableParent);

        Disconnect("area_entered", this, nameof(OnArea2DAreaEntered));
        Disconnect("area_exited", this, nameof(OnArea2DAreaExited));

        if (!HurtBox.IsConnected(nameof(HurtBox.DidDamage), this, nameof(OnCollision)))
            HurtBox.Connect(nameof(HurtBox.DidDamage), this, nameof(OnCollision));

        if (!WallDetect.IsConnected("body_entered", this, nameof(OnWallDetected)))
            WallDetect.Connect("body_entered", this, nameof(OnWallDetected));
    }

    public override void _PhysicsProcess(float delta)
    {
        ThrowVelocity = new Vector2(ThrowVelocity.x, ThrowVelocity.y + Gravity * delta);
        ThrowableParent.Position += ThrowVelocity * delta;
        ThrowSpeedWallDetect += Gravity * delta;
        WallDetect.Position = new Vector2(WallDetect.Position.x, WallDetect.Position.y - ThrowSpeedWallDetect * delta);
    }

    public async void SetState(string state, Vector2 throwDirection)
    {
        ThrowDirection = throwDirection;
        Vector2 globalPosition = ThrowableParent.GlobalPosition;
        SceneTree sceneTree = GetTree();
        ThrowableParent.GetParent().RemoveChild(ThrowableParent);

        // add child needs to be deferred but on doing that, the onready variables like Timer become null because the
        // add child is happening later. so better to wait for the idle frame and do everything after that so that the
        // add child happens first before accessing the node variables like Timer
        await ToSignal(sceneTree, "idle_frame");

        GlobalPlayerManager.Instance.Player.GetParent().AddChild(ThrowableParent);

        // VERY IMPORTANT
        // set only throwable's shapes to active not the static body of throwable parent otherwise the walldetect will
        // will detect the parent also
        SetCollisionBodies(this, false);
        ThrowableParent.GlobalPosition = globalPosition;

        // let wall detect move on the ground
        WallDetect.GlobalPosition = GlobalPlayerManager.Instance.Player.GlobalPosition;

        float playerToItemVectorMagnitude = GlobalPlayerManager.Instance.Player.GlobalPosition.DistanceTo(globalPosition);
        Timer.WaitTime = Mathf.Sqrt(2 * playerToItemVectorMagnitude / Gravity);
        SpeedAtTouchDown = Gravity * Timer.WaitTime;
        Timer.Start();

        SetPhysicsProcess(true);
        GlobalPlayerManager.Instance.Player.Throwable = null;

        if (state == "throw")
        {
            HurtBox.Monitorable = true;
            WallDetect.Monitoring = true;

            // VERY IMPORTANT
            // i dont know but for tilemap detection its not sufficient to put monitoring true, we need monitorable too
            WallDetect.Monitorable = true;

            Vector2 playerToItemVector = playerToItemVectorMagnitude * GlobalPlayerManager.Instance.Player.GlobalPosition.DirectionTo(globalPosition);
            Vector2 landLocationFromPlayerFeet = ThrowDistance * 32 * ThrowDirection;
            Vector2 finalVector = landLocationFromPlayerFeet - playerToItemVector;
            ThrowVelocity = new Vector2(finalVector.x, finalVector.y - 0.5f * Gravity * Mathf.Pow(Timer.WaitTime, 2)) / Timer.WaitTime;
            return;
        }

        ThrowVelocity = Vector2.Zero;
    }

    private void SetupAreas()
    {
        HurtBox.Monitorable = false;
        WallDetect.Monitoring = false;
        WallDetect.Monitorable = false;

        foreach (Node child in GetChildren())
        {
            if (child is CollisionShape2D collisionShape2D)
            {
                WallDetect.AddChild(collisionShape2D.Duplicate());
                HurtBox.AddChild(collisionShape2D.Duplicate());
            }
        }
    }

    private void SetCollisionBodies(Node parent, bool value)
    {
        Godot.Collections.Array children = parent.GetChildren();

        foreach (Node c in children)
        {
            if (c is CollisionShape2D collisionShape2D)
                collisionShape2D.Disabled = value;

            SetCollisionBodies(c, value);
        }
    }

    private void Destroy()
    {
        SetPhysicsProcess(false);
        AnimationPlayer.Play("destroy");
    }

    protected virtual void OnCollision()
    {
        Destroy();
    }

    protected virtual void OnTimeout()
    {
        Destroy();
    }

    private void OnAnimationPlayerAnimationFinished(string animName)
    {
        if (IsConnected("area_entered", this, nameof(OnArea2DAreaEntered)))
            Disconnect("area_entered", this, nameof(OnArea2DAreaEntered));

        if (IsConnected("area_exited", this, nameof(OnArea2DAreaExited)))
            Disconnect("area_exited", this, nameof(OnArea2DAreaExited));

        ThrowableParent.QueueFree();
    }

    private void OnWallDetected(Node body)
    {
        OnCollision();
    }
}
