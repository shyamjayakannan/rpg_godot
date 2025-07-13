using Godot;

public partial class YSortAnchor : Area2D
{
    // Exports
    [Export]
    private float defaultDifference;

    // methods
    public override void _Ready()
    {
        CallDeferred(MethodName.SetChild, defaultDifference);
    }

    private void SetChild(float difference)
    {
        ((YSortHandler)GetParent().GetParent()).SetChild(difference);
    }
}
