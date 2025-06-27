using System.Collections.Generic;
using System.Linq;
using Godot;

[Tool]
public class PatrolBehavior : NPCBehavior
{
    // Exports
    [Export]
    private readonly float walkSpeed = 30.0f;

    // private
    private bool alreadyCalled = false;
    private readonly List<PatrolLocation> patrolLocations = new List<PatrolLocation>();
    private PatrolLocation target;
    private int currentIndex = 0;
    private readonly Color[] colors = new Color[] { new Color(0, 0, 1), new Color(0, 1, 0), new Color(1, 0, 0), new Color(0, 1, 1), new Color(1, 0, 1), new Color(1, 1, 0) };

    // methods
    public override void _Ready()
    {
        base._Ready();
        UpdatePatrolLocations();

        if (Engine.EditorHint)
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
        if (!Engine.EditorHint)
            return;

        if (what != NotificationChildOrderChanged)
            return;

        UpdatePatrolLocations();
    }

    public override void _Process(float delta)
    {
        if (Engine.EditorHint)
            return;

        if (!alreadyCalled && Npc.GlobalPosition.DistanceTo(target.TargetPosition) < 1)
            Start();
    }

    public void UpdatePatrolLocations()
    {
        patrolLocations.Clear();
        Godot.Collections.Array children = GetChildren();

        for (int i = 0; i < children.Count; i++)
        {
            PatrolLocation patrolLocation = (PatrolLocation)children[i];
            patrolLocations.Add(patrolLocation);

            if (!Engine.EditorHint)
                continue;

            ChangePatrolLocations(i);
        }

        if (children.Count > 0 && Engine.EditorHint)
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

        if (!patrolLocations[i].IsConnected(nameof(PatrolLocation.TransformChanged), this, nameof(OnTransformChanged)))
            patrolLocations[i].Connect(nameof(PatrolLocation.TransformChanged), this, nameof(OnTransformChanged));
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

        GetTree().CreateTimer(target.WaitTime, false).Connect("timeout", this, nameof(Start2));
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
