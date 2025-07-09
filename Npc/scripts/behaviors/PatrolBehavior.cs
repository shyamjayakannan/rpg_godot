using System.Collections.Generic;
using System.Linq;
using Godot;

[Tool]
public partial class PatrolBehavior : NPCBehavior
{
    // Exports
    [Export]
    private float walkSpeed = 30.0f;

    // private
    private bool alreadyCalled = false;
    private List<PatrolLocation> patrolLocations = new();
    private PatrolLocation target;
    private int currentIndex = 0;
    private Color[] colors = new Color[] { new(0, 0, 1), new(0, 1, 0), new(1, 0, 0), new(0, 1, 1), new(1, 0, 1), new(1, 1, 0) };

    // methods
    public override void _Ready()
    {
        base._Ready();
        UpdatePatrolLocations();

        if (Engine.IsEditorHint())
            return;

        if (patrolLocations.Count == 0)
        {
            SetProcess(false);
            return;
        }

        target = patrolLocations[0];
        Npc.GlobalPosition = target.TargetPosition;
    }

    public override void _Notification(int what)
    {
        if (!Engine.IsEditorHint())
            return;

        if (what != NotificationChildOrderChanged)
            return;

        UpdatePatrolLocations();
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
            return;

        if (!alreadyCalled && Npc.GlobalPosition.DistanceTo(target.TargetPosition) < 1)
            Start();
    }

    public void UpdatePatrolLocations()
    {
        patrolLocations.Clear();
        Godot.Collections.Array<Node> children = GetChildren();

        for (int i = 0; i < children.Count; i++)
        {
            PatrolLocation patrolLocation = (PatrolLocation)children[i];
            patrolLocations.Add(patrolLocation);

            if (!Engine.IsEditorHint())
                continue;

            ChangePatrolLocations(i);
        }

        if (children.Count > 0 && Engine.IsEditorHint())
            patrolLocations.Last().UpdateLine(patrolLocations[0].GlobalPosition);
    }

    private void OnTransformChanged(int i)
    {
        if (i != 0)
            patrolLocations[i - 1].UpdateLine(patrolLocations[i].GlobalPosition);
        else
            patrolLocations.Last().UpdateLine(patrolLocations[0].GlobalPosition);

        if (i != patrolLocations.Count - 1)
            patrolLocations[i].UpdateLine(patrolLocations[i + 1].GlobalPosition);
        else
            patrolLocations.Last().UpdateLine(patrolLocations[0].GlobalPosition);
    }

    private void ChangePatrolLocations(int i)
    {
        patrolLocations[i].UpdateLabel($"{i}");
        patrolLocations[i - 1 < 0 ? 0 : i - 1].UpdateLine(patrolLocations[i].GlobalPosition);
        patrolLocations[i].Modulate = colors[i % colors.Length];
        patrolLocations[i].Index = i;

        if (!patrolLocations[i].IsConnected(PatrolLocation.SignalName.TransformChanged, new(this, MethodName.OnTransformChanged)))
            patrolLocations[i].Connect(PatrolLocation.SignalName.TransformChanged, new(this, MethodName.OnTransformChanged));
    }

    protected override void Start()
    {
        alreadyCalled = true;

        if (!Npc.DoBehavior || patrolLocations.Count < 2)
        {
            alreadyCalled = false;
            return;
        }

        // idle
        Npc.GlobalPosition = target.TargetPosition;
        Npc.State = "idle";
        Npc.Velocity = Vector2.Zero;
        Npc.UpdateAnimation();

        GetTree().CreateTimer(target.WaitTime, false).Connect(SceneTreeTimer.SignalName.Timeout, new(this, MethodName.Start2));
    }

    private void Start2()
    {
        // set wanderbehavior's Start() for reason
        if (!Npc.DoBehavior || patrolLocations.Count < 2)
        {
            alreadyCalled = false;
            return;
        }

        if (++currentIndex == patrolLocations.Count)
            currentIndex = 0;

        target = patrolLocations[currentIndex];

        // walk
        Npc.State = "walk";
        Vector2 direction = Npc.GlobalPosition.DirectionTo(target.TargetPosition);
        Npc.Velocity = walkSpeed * direction;
        Npc.UpdateDirection(direction);
        Npc.UpdateAnimation();

        alreadyCalled = false;
    }
}
