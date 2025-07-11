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
    protected float Timer { get; set; }
    protected Node2D ThrowableParent { get; private set; }
    protected AnimationPlayer AnimationPlayer { get; private set; }
    protected Vector2 ThrowVelocity { get; set; }
    protected Vector2 ThrowDirection { get; set; }
    protected HurtBox HurtBox { get; private set; }
    protected Sprite2D Shadow { get; private set; }

    // private
    private Sprite2D sprite2D;
    private float offset;

    // methods
    public override void _Ready()
    {
        HurtBox = GetNode<HurtBox>("HurtBox");
        WallDetect = GetNode<Area2D>("WallDetect");
        ThrowableParent = (Node2D)GetParent();
        AnimationPlayer = ThrowableParent.GetNode<AnimationPlayer>("AnimationPlayer");
        Shadow = ThrowableParent.GetNodeOrNull<Sprite2D>("Shadow");
        sprite2D = ThrowableParent.GetNodeOrNull<Sprite2D>("Sprite2D");
        SetupAreas();
        SetPhysicsProcess(false);

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
            Shadow.Position -= Position;
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
        float floatDelta = (float)delta;
        Timer -= floatDelta;

        if (Timer < 0)
        {
            OnTimeout();
            return;
        }

        ThrowVelocity = new(ThrowVelocity.X, ThrowVelocity.Y + Gravity * floatDelta);
        Vector2 toAdd = ThrowVelocity * floatDelta;
        ThrowableParent.Position += toAdd;
        ThrowSpeedWallDetect += Gravity * floatDelta;
        sprite2D.Offset = new(sprite2D.Offset.X, sprite2D.Offset.Y + ThrowSpeedWallDetect * floatDelta);
        WallDetect.Position = new(WallDetect.Position.X, WallDetect.Position.Y - ThrowSpeedWallDetect * floatDelta);
        sprite2D.Position = new(WallDetect.Position.X, WallDetect.Position.Y - offset);
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
        await ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);

        GlobalPlayerManager.Instance.Player.GetParent().AddChild(ThrowableParent);

        // VERY IMPORTANT
        // set only throwable's shapes to active not the static body of throwable parent otherwise the walldetect will
        // will detect the parent also
        SetCollisionBodies(this, false);
        ThrowableParent.GlobalPosition = globalPosition;

        // let wall detect move on the ground
        WallDetect.GlobalPosition = GlobalPlayerManager.Instance.Player.GlobalPosition;
        offset = sprite2D.Offset.Y;
        sprite2D.GlobalPosition = new(GlobalPlayerManager.Instance.Player.GlobalPosition.X, GlobalPlayerManager.Instance.Player.GlobalPosition.Y - offset);
        sprite2D.Offset = new(sprite2D.Offset.X, globalPosition.Y - sprite2D.GlobalPosition.Y);

        float playerToItemVectorMagnitude = GlobalPlayerManager.Instance.Player.GlobalPosition.DistanceTo(globalPosition);
        Timer = Mathf.Sqrt(2 * playerToItemVectorMagnitude / Gravity);
        SpeedAtTouchDown = Gravity * Timer;

        SetPhysicsProcess(true);
        GlobalPlayerManager.Instance.Player.Throwable = null;

        if (state == "throw")
        {
            Shadow?.Show();
            HurtBox.Monitorable = true;
            WallDetect.Monitoring = true;

            // VERY IMPORTANT
            // i dont know but for tilemap detection its not sufficient to put monitoring true, we need monitorable too
            WallDetect.Monitorable = true;

            Vector2 playerToItemVector = playerToItemVectorMagnitude * GlobalPlayerManager.Instance.Player.GlobalPosition.DirectionTo(globalPosition);
            Vector2 landLocationFromPlayerFeet = ThrowDistance * 32 * ThrowDirection;
            Vector2 finalVector = landLocationFromPlayerFeet - playerToItemVector;
            ThrowVelocity = new Vector2(finalVector.X, finalVector.Y - 0.5f * Gravity * Mathf.Pow(Timer, 2)) / (float)Timer;
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

    private void OnAnimationPlayerAnimationFinished(string _)
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
