using Godot;

namespace Rpg
{
    public partial class PersistentDataHandler : Node
    {
        // Signals
        [Signal]
        public delegate void DataLoadedEventHandler(bool value);

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
            EmitSignal(SignalName.DataLoaded, value);
        }

        private string GetItemName()
        {
            return $"{GetTree().CurrentScene.SceneFilePath}/{GetParent().Name}/{Name}";
        }
    }
}
