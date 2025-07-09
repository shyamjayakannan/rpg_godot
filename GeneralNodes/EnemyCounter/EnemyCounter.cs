using Godot;

/// MAKE SURE ITEMDROPPER IS ABOVE ENEMYCOUNTER IN THE SCENETREE BECAUSE ITS ONREADY NEEDS TO BE CALLED FIRST.
/// THIS IS BECAUSE ITEMDROPPER'S ONREADY CONNECTS TO THE SIGNAL THAT IS FIRED BY THE GETVALUE CALL IN
/// ENEMYCOUNTER'S ONREADY. SIGNAL NEEDS TO BE CONNECTED BEFORE FIRING OTHERWISE WONT CATCH
public partial class EnemyCounter : Node2D
{
    // private
    private int enemyCount = 0;
    private PersistentDataHandler persistentDataHandler;

    // methods
    public override void _Ready()
    {
        persistentDataHandler = GetNode<PersistentDataHandler>("PersistentDataHandler");
        persistentDataHandler.Connect(PersistentDataHandler.SignalName.DataLoaded, new(this, MethodName.SetEnemies));
        persistentDataHandler.GetValue();
    }

    private void CheckAllEnemiesDestroyed(Node child)
    {
        if (child is not Enemy)
            return;

        enemyCount--;

        if (enemyCount == 0)
        {
            GlobalSignalManager.Instance.EmitSignal(GlobalSignalManager.SignalName.EnemiesDestroyed, false);
            persistentDataHandler.SetValue();
        }
    }

    private void SetEnemies(bool alreadyDestroyed)
    {
        if (alreadyDestroyed)
        {
            QueueFree();
            GlobalSignalManager.Instance.EmitSignal(GlobalSignalManager.SignalName.EnemiesDestroyed, true);
            return;
        }

        enemyCount = GetChildCount();
        Connect(Node.SignalName.ChildExitingTree, new(this, MethodName.CheckAllEnemiesDestroyed));
    }
}
