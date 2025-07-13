using Godot;

public partial class YSortTransition : Area2D
{
    // private
    private float ySortOrigin;

    // methods
    public override void _Ready()
    {
        TileMapLayer parent = (TileMapLayer)GetParent();
        ySortOrigin = parent.YSortOrigin + parent.TileSet.TileSize.Y / 2;
        Connect(Area2D.SignalName.AreaEntered, new(this, MethodName.OnAreaEntered));
    }

    private void OnAreaEntered(Area2D area2D)
    {
        if (area2D is YSortAnchor)
            ((YSortHandler)area2D.GetParent().GetParent()).YSortOrigin = ySortOrigin;
    }
}
