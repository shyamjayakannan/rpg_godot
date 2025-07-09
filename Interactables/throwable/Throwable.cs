using Godot;

public partial class Throwable : Interactables
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
    protected Sprite2D Shadow { get; private set; }

    // methods
    public override void _Ready()
    {
        HurtBox = GetNode<HurtBox>("HurtBox");
        WallDetect = GetNode<Area2D>("WallDetect");
        Timer = GetNode<Timer>("Timer");
        ThrowableParent = (Node2D)GetParent();
        AnimationPlayer = ThrowableParent.GetNode<AnimationPlayer>("AnimationPlayer");
        Shadow = ThrowableParent.GetNodeOrNull<Sprite2D>("Shadow");
        SetupAreas();
        SetPhysicsProcess(false);

        Timer.Connect(Timer.SignalName.Timeout, new(this, MethodName.OnTimeout));
        AnimationPlayer.Connect(AnimationMixer.SignalName.AnimationFinished, new(this, MethodName.OnAnimationPlayerAnimationFinished));
        Connect(Area2D.SignalName.AreaEntered, new(this, Interactables.MethodName.OnArea2DAreaEntered));
        Connect(Area2D.SignalName.AreaExited, new(this, Interactables.MethodName.OnArea2DAreaExited));
    }

    public override void OnInteractPressed()
    {
        if (GlobalPlayerManager.Instance.Player.Throwable != null)
            return;

        SetCollisionBodies(ThrowableParent, true);
        ThrowableParent.GetParent()?.RemoveChild(ThrowableParent);
        GlobalPlayerManager.Instance.Player.PickupItem(this, ThrowableParent);

        // let shadow move on ground
        if (Shadow != null)
        {
            ThrowableParent.RemoveChild(Shadow);
            WallDetect.AddChild(Shadow);
            Shadow.Hide();
        }

        Disconnect(Area2D.SignalName.AreaEntered, new(this, Interactables.MethodName.OnArea2DAreaEntered));
        Disconnect(Area2D.SignalName.AreaExited, new(this, Interactables.MethodName.OnArea2DAreaExited));

        if (!HurtBox.IsConnected(HurtBox.SignalName.DidDamage, new(this, MethodName.OnCollision)))
            HurtBox.Connect(HurtBox.SignalName.DidDamage, new(this, MethodName.OnCollision));

        if (!WallDetect.IsConnected(Area2D.SignalName.BodyEntered, new(this, MethodName.OnWallDetected)))
            WallDetect.Connect(Area2D.SignalName.BodyEntered, new(this, MethodName.OnWallDetected));
    }

    public override void _PhysicsProcess(double delta)
    {
        ThrowVelocity = new Vector2(ThrowVelocity.X, ThrowVelocity.Y + Gravity * (float)delta);
        ThrowableParent.Position += ThrowVelocity * (float)delta;
        ThrowSpeedWallDetect += Gravity * (float)delta;
        WallDetect.Position = new Vector2(WallDetect.Position.X, WallDetect.Position.Y - ThrowSpeedWallDetect * (float)delta);
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
        Shadow?.Show();

        float playerToItemVectorMagnitude = GlobalPlayerManager.Instance.Player.GlobalPosition.DistanceTo(globalPosition);
        Timer.WaitTime = Mathf.Sqrt(2 * playerToItemVectorMagnitude / Gravity);
        SpeedAtTouchDown = Gravity * (float)Timer.WaitTime;
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
            ThrowVelocity = new Vector2(finalVector.X, finalVector.Y - 0.5f * Gravity * Mathf.Pow((float)Timer.WaitTime, 2)) / (float)Timer.WaitTime;
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

    private static void SetCollisionBodies(Node parent, bool value)
    {
        Godot.Collections.Array<Node> children = parent.GetChildren();

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
        Shadow.Hide();
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
        if (IsConnected(Area2D.SignalName.AreaEntered, new(this, Interactables.MethodName.OnArea2DAreaEntered)))
            Disconnect(Area2D.SignalName.AreaEntered, new(this, Interactables.MethodName.OnArea2DAreaEntered));

        if (IsConnected(Area2D.SignalName.AreaExited, new(this, Interactables.MethodName.OnArea2DAreaExited)))
            Disconnect(Area2D.SignalName.AreaExited, new(this, Interactables.MethodName.OnArea2DAreaExited));

        ThrowableParent.QueueFree();
    }

    private void OnWallDetected(Node body)
    {
        OnCollision();
    }
}
