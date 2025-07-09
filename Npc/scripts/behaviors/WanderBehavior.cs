using System.Linq;
using Godot;

[Tool]
public partial class WanderBehavior : NPCBehavior
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
        if (Engine.IsEditorHint())
            return;

        base._Ready();
        Npc.Connect(Npc.SignalName.DoBehaviorEnabled, new(this, MethodName.Start));
        area2D = GetNode<Area2D>("Area2D");
        area2D.CollisionMask = 8;
        RemoveChild(area2D);
        area2D.GlobalPosition = GlobalPosition;
        Npc.GetParent().CallDeferred("add_child", area2D);

        Connect(Node.SignalName.TreeExited, new(this, MethodName.Destroy));
        area2D.Connect(Area2D.SignalName.BodyExited, new(this, MethodName.OnAreaExited));
    }

    private void Destroy()
    {
        area2D.QueueFree();
    }

    public override string[] _GetConfigurationWarnings()
    {
        int count = 0;

        foreach (Area2D child in GetChildren().Cast<Area2D>())
            count++;

        return count < 1 ? new[] { "please add one area2d node" } : count > 1 ? new[] { "please add only one are2d node" } : System.Array.Empty<string>();
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

        GetTree().CreateTimer(idleDuration * GD.Randf(), false).Connect(SceneTreeTimer.SignalName.Timeout, new(this, MethodName.Start2));
    }

    private void Start2()
    {
        if (!Npc.DoBehavior)
            return;

        // walk
        Npc.State = "walk";
        Vector2 direction = directions[GD.Randi() % directions.Length];
        Npc.Velocity = wanderSpeed * direction;
        Npc.UpdateDirection(direction);
        Npc.UpdateAnimation();

        GetTree().CreateTimer(wanderDuration * GD.Randf(), false).Connect(SceneTreeTimer.SignalName.Timeout, new(this, MethodName.Start));
    }

}
