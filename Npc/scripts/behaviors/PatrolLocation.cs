using Godot;

[Tool]
public partial class PatrolLocation : Node2D
{
    // Signals
    [Signal]
    public delegate void TransformChangedEventHandler(int index);

    // Exports
    [Export]
    public float WaitTime
    {
        get => waitTime;
        set
        {
            waitTime = value;

            if (Engine.IsEditorHint())
                UpdateWaitTime();
        }
    }

    // private
    private float waitTime = 0.0f;
    private Label label;
    private Label label2;
    private Line2D line2D;
    private Transform2D transform;

    // properties
    public int Index { get; set; }
    public Vector2 TargetPosition { get; private set; }

    // methods
    public override void _Ready()
    {
        label = GetNode<Label>("Sprite2D/Label");
        label2 = GetNode<Label>("Sprite2D/Label2");
        line2D = GetNode<Line2D>("Sprite2D/Line2D");
        UpdateWaitTime();
        transform = Transform;
        TargetPosition = GlobalPosition;

        ((PatrolBehavior)GetParent()).UpdatePatrolLocations();

        if (Engine.IsEditorHint())
            return;

        GetNode<Sprite2D>("Sprite2D").QueueFree();
    }

    public override void _EnterTree()
    {
        if (!Engine.IsEditorHint())
            return;

        SetNotifyTransform(true);
    }

    public override void _Notification(int what)
    {
        if (!Engine.IsEditorHint())
            return;

        switch (what)
        {
            case (int)NotificationTransformChanged:
                if (transform == Transform)
                    return;

                transform = Transform;
                EmitSignal(SignalName.TransformChanged, Index);
                break;
        }
    }

    public void UpdateLabel(string s)
    {
        if (label != null)
            label.Text = s;
    }

    public void UpdateLine(Vector2 nextLocation)
    {
        // VERY IMPORTANT
        // since line2D.Points is a property, you can only set its value at the current level, not inside
        // so you can only reassign the entire points array, not a specific point. for that use this function
        line2D?.SetPointPosition(1, nextLocation - GlobalPosition);
    }

    private void UpdateWaitTime()
    {
        if (label2 != null)
            label2.Text = $"wait: {WaitTime}s";
    }
}
