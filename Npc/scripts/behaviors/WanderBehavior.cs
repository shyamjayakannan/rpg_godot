using Godot;

[Tool]
public class WanderBehavior : NPCBehavior
{
    // Exports
    [Export]
    private float wanderSpeed = 30.0f;
    [Export]
    private float wanderDuration = 5.0f;
    [Export]
    private float idleDuration = 1.0f;

    // private
    private Vector2[] directions = new Vector2[] { Vector2.Up, Vector2.Right, Vector2.Down, Vector2.Left };
    private Area2D area2D;

    // methods
    public override void _Ready()
    {
        if (Engine.EditorHint)
            return;

        base._Ready();
        Npc.Connect(nameof(Npc.DoBehaviorEnabled), this, nameof(Start));
        area2D = GetNode<Area2D>("Area2D");
        area2D.CollisionMask = 8;
        RemoveChild(area2D);
        area2D.GlobalPosition = GlobalPosition;
        Npc.GetParent().CallDeferred("add_child", area2D);

        Connect("tree_exited", this, nameof(Destroy));
        area2D.Connect("body_exited", this, nameof(OnAreaExited));
    }

    private void Destroy()
    {
        area2D.QueueFree();
    }

    public override string _GetConfigurationWarning()
    {
        int count = 0;

        foreach (Area2D child in GetChildren())
            count++;

        return count < 1 ? "please add one area2d node" : count > 1 ? "please add only one are2d node" : "";
    }

    private void OnAreaExited(Node body)
    {
        if (body == Npc)
        {
            Npc.SetPhysicsProcess(false);
            Npc.Velocity *= -1;
            Npc.UpdateDirection(-Npc.Direction);
            Npc.UpdateAnimation();
            Npc.SetPhysicsProcess(true);
        }
    }

    protected override void Start()
    {
        if (!Npc.DoBehavior)
            return;

        // idle
        Npc.State = "idle";
        Npc.Velocity = Vector2.Zero;
        Npc.UpdateAnimation();

        GetTree().CreateTimer(idleDuration * GD.Randf(), false).Connect("timeout", this, nameof(Start2));
    }

    private void Start2()
    {
        // walk
        Npc.State = "walk";
        Vector2 direction = directions[GD.Randi() % directions.Length];
        Npc.Velocity = wanderSpeed * direction;
        Npc.UpdateDirection(direction);
        Npc.UpdateAnimation();

        GetTree().CreateTimer(wanderDuration * GD.Randf(), false).Connect("timeout", this, nameof(Start));
    }

}
