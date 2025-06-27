using Godot;

/// MAKE SURE ITEMDROPPER IS ABOVE ENEMYCOUNTER IN THE SCENETREE BECAUSE ITS ONREADY NEEDS TO BE CALLED FIRST.
/// THIS IS BECAUSE ITEMDROPPER'S ONREADY CONNECTS TO THE SIGNAL THAT IS FIRED BY THE GETVALUE CALL IN
/// ENEMYCOUNTER'S ONREADY. SIGNAL NEEDS TO BE CONNECTED BEFORE FIRING OTHERWISE WONT CATCH
public class EnemyCounter : Node2D
{
    // private
    private int enemyCount = 0;
    private PersistentDataHandler persistentDataHandler;

    // methods
    public override void _Ready()
    {
        persistentDataHandler = GetNode<PersistentDataHandler>("PersistentDataHandler");
        persistentDataHandler.Connect(nameof(PersistentDataHandler.DataLoaded), this, nameof(SetEnemies));
        persistentDataHandler.GetValue();
    }

    private void CheckAllEnemiesDestroyed(Node child)
    {
        if (!(child is Enemy))
            return;

        enemyCount--;

        if (enemyCount == 0)
        {
            GlobalSignalManager.Instance.EmitSignal(nameof(GlobalSignalManager.EnemiesDestroyed), false);
            persistentDataHandler.SetValue();
        }
    }

    private void SetEnemies(bool alreadyDestroyed)
    {
        if (alreadyDestroyed)
        {
            QueueFree();
            GlobalSignalManager.Instance.EmitSignal(nameof(GlobalSignalManager.EnemiesDestroyed), true);
            return;
        }

        enemyCount = GetChildCount();
        Connect("child_exiting_tree", this, nameof(CheckAllEnemiesDestroyed));
    }
}
