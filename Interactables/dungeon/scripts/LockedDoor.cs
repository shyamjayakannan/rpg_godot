using Godot;

namespace Rpg
{
    public partial class LockedDoor : Interactables
    {
        // Exports
        [Export]
        private Items key;

        // private
        private bool isOpen = false;
        private AnimationPlayer animationPlayer;
        private Area2D interactArea;
        private PersistentDataHandler persistentDataHandler;

        // methods
        public override void _Ready()
        {
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            interactArea = GetNode<Area2D>("Area2D");
            persistentDataHandler = GetNode<PersistentDataHandler>("PersistentDataHandler");

            interactArea.Connect(Area2D.SignalName.AreaEntered, new(this, Interactables.MethodName.OnArea2DAreaEntered));
            interactArea.Connect(Area2D.SignalName.AreaExited, new(this, Interactables.MethodName.OnArea2DAreaExited));

            persistentDataHandler.Connect(PersistentDataHandler.SignalName.DataLoaded, new(this, MethodName.SetDoorState));
            persistentDataHandler.GetValue();
        }

        public override void OnInteractPressed()
        {
            if (isOpen)
                return;

            if (GlobalPlayerManager.Instance.PlayerInventory.RemoveItem(key))
            {
                animationPlayer.Play("openDoor");
                persistentDataHandler.SetValue();
            }
            else
                animationPlayer.Play("closedDoor");
        }

        private void SetDoorState(bool value)
        {
            isOpen = value;

            if (isOpen)
                animationPlayer.Play("opened");
            else
                animationPlayer.Play("closed");
        }
    }
}
