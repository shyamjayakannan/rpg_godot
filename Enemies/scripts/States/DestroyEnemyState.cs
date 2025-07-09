using Godot;

public partial class DestroyEnemyState : EnemyState
{
	// Exports
	[Export]
	private float knockbackSpeed = 200f;
	[Export]
	private float deceleration = 10f;
	[Export]
	private DropData[] drops = System.Array.Empty<DropData>();

	// private
	private Vector2 direction;
	private Vector2 damagePosition;
	private PackedScene itemPickupScene = GD.Load<PackedScene>("res://Items/itemPickup/ItemPickup.tscn");

	// methods
	public override void Init()
	{
		Enemy.Connect(Enemy.SignalName.EnemyDestroyed, new(this, MethodName.OnEnemyDestroyed));
	}

	public override void Enter()
	{
		direction = Enemy.GlobalPosition.DirectionTo(damagePosition);
		Enemy.SetDirection(direction);
		Enemy.Velocity = direction * (-knockbackSpeed);
		Enemy.UpdateAnimation("destroy");
		Enemy.AnimationPlayer.Connect(AnimationMixer.SignalName.AnimationFinished, new(this, MethodName.OnAnimationPlayerAnimationFinished));
		Enemy.Invulnerable = true;
		DropItems();
		GlobalPlayerManager.Instance.Player.UpdateXP(Enemy.RewardXp);
	}

	public override void Exit()
	{
		Enemy.Invulnerable = false;
	}

	public override EnemyState Process(double delta)
	{
		Enemy.Velocity *= 1 - deceleration * (float)delta;

		return null;
	}

	private void OnEnemyDestroyed(HurtBox hurtBox)
	{
		damagePosition = hurtBox.GlobalPosition;
		StateMachine.ChangeState(this);
	}

	private void OnAnimationPlayerAnimationFinished(string anim_name)
	{
		if (StateMachine.CurrentState == this)
			Enemy.QueueFree();
	}

	private void DropItems()
	{
		foreach (DropData drop in drops)
		{
			for (int j = 0; j < drop.GetDropCount(); j++)
			{
				ItemPickup itemPickup = (ItemPickup)itemPickupScene.Instantiate();
				itemPickup.Item = drop.item;
				itemPickup.GlobalPosition = Enemy.GlobalPosition;
				itemPickup.IsDroppedItem = true;
				itemPickup.Velocity = new Vector2(2, 2).Rotated((float)GD.RandRange(-1.5, 1.5)) * (float)GD.RandRange(0.9, 1.5);
				Enemy.GetParent().CallDeferred("add_child", itemPickup);
			}
		}
	}
}
