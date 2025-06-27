using Godot;

public class Throwable : Intercatables
{
    // Exports
    [Export]
    private float gravity = 980;
    [Export]
    private float throwSpeed = 400;

    // private
    private bool pickedUp = false;
    private Node2D throwable;
    private HurtBox hurtBox;
    private Vector2 throwVelocity;
    private AnimationPlayer animationPlayer;
    private Timer timer;
    private StaticBody2D staticBody2D;

    // methods
    public override void _Ready()
    {
        hurtBox = GetNode<HurtBox>("HurtBox");
        timer = GetNode<Timer>("Timer");
        throwable = (Node2D)GetParent();
        staticBody2D = throwable.GetNode<StaticBody2D>("StaticBody2D");
        animationPlayer = throwable.GetNode<AnimationPlayer>("AnimationPlayer");
        SetupHurtBox();
        SetPhysicsProcess(false);

        timer.Connect("timeout", this, nameof(OnTimerTimeout));
        animationPlayer.Connect("animation_finished", this, nameof(OnAnimationPlayerAnimationFinished));
        Connect("area_entered", this, nameof(OnArea2DAreaEntered));
        Connect("area_exited", this, nameof(OnArea2DAreaExited));
    }

    protected override void OnInteractPressed()
    {
        if (pickedUp)
            return;

        pickedUp = true;
        SetCollisionBodies(throwable, true);
        throwable.GetParent()?.RemoveChild(throwable);
        GlobalPlayerManager.Instance.Player.PickupItem(this, throwable);

        Disconnect("area_entered", this, nameof(OnArea2DAreaEntered));
        Disconnect("area_exited", this, nameof(OnArea2DAreaExited));
        hurtBox.Connect(nameof(HurtBox.DidDamage), this, nameof(OnHurtBoxDidDamage));
    }

    public override void _PhysicsProcess(float delta)
    {
        throwVelocity.y += gravity * delta;
        throwable.Position += throwVelocity * delta;
    }

    public async void SetState(string state, Vector2 ThrowDirection)
    {
        Vector2 globalPosition = throwable.GlobalPosition;
        SceneTree sceneTree = GetTree();
        throwable.GetParent().RemoveChild(throwable);

        // add child needs to be deferred but on doing that, the onready variables like timer become null because the
        // add child is happening later. so better to wait for the idle frame and do everything after that so that the
        // add child happens first before accessing the node variables like timer
        await ToSignal(sceneTree, "idle_frame");

        GlobalPlayerManager.Instance.Player.GetParent().AddChild(throwable);
        SetCollisionBodies(throwable, false);
        throwable.GlobalPosition = globalPosition;
        timer.WaitTime = Mathf.Sqrt(2 * staticBody2D.GlobalPosition.DistanceTo(GlobalPlayerManager.Instance.Player.GlobalPosition) / gravity);
        timer.Start();

        SetPhysicsProcess(true);

        if (state == "throw")
        {
            hurtBox.Monitorable = true;
            throwVelocity = ThrowDirection * throwSpeed;
            return;
        }

        throwVelocity = Vector2.Zero;
    }

    private void SetupHurtBox()
    {
        hurtBox.Monitorable = false;

        foreach (Node child in GetChildren())
            if (child is CollisionShape2D collisionShape2D)
                hurtBox.AddChild(collisionShape2D.Duplicate());
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

    private void OnTimerTimeout()
    {
        Destroy();
    }

    private void Destroy()
    {
        SetPhysicsProcess(false);
        animationPlayer.Play("destroy");
    }

    private void OnAnimationPlayerAnimationFinished(string animName)
    {
        throwable.QueueFree();
    }

    private void OnHurtBoxDidDamage()
    {
        Destroy();
    }
}
