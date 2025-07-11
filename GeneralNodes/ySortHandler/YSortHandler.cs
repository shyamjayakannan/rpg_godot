using Godot;

[Tool]
public partial class YSortHandler : Node
{
    // Exports
    [Export]
    private float YSortOrigin
    {
        get => ySortOrigin;
        set => SetObject(value);
    }

    // private
    private float ySortOrigin = 0;
    private Node2D parent;

    // methods
    public override void _Ready()
    {
        Node p = GetParent();
        parent = p is Node2D node2D ? node2D : null;
        SetObject(YSortOrigin);
    }

    private void SetObject(float newYSortOrigin)
    {
        if (parent == null)
        {
            ySortOrigin = newYSortOrigin;
            return;
        }

        float difference = newYSortOrigin - ySortOrigin;
        ySortOrigin = newYSortOrigin;
        parent.GlobalPosition = new(parent.GlobalPosition.X, parent.GlobalPosition.Y + difference);

        foreach (Node child in parent.GetChildren())
            if (child is Node2D node2D)
                node2D.Position = new(node2D.Position.X, node2D.Position.Y - difference);
    }
}