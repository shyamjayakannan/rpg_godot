using Godot;

public class PersistentDataHandler : Node
{
    // Signals
    [Signal]
    public delegate void DataLoaded(bool value);

    // private
    private bool value = false;

    // methods
    public override void _Ready()
    {
        GetValue();
    }

    public void SetValue()
    {
        GlobalSaveManager.Instance.AddPersistentValue(GetItemName());
    }

    public void UnsetValue()
    {
        if (IsInsideTree())
            GlobalSaveManager.Instance.RemovePersistentValue(GetItemName());
    }

    public void GetValue()
    {
        value = GlobalSaveManager.Instance.CheckPersistentValue(GetItemName());
        EmitSignal(nameof(DataLoaded), value);
    }

    private string GetItemName()
    {
        return $"{GetTree().CurrentScene.Filename}/{GetParent().Name}/{Name}";
    }
}
