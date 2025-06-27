using Godot;

public class DestroyEnemyState : EnemyState
{
	// Exports
	[Export]
	private readonly float knockbackSpeed = 200f;
	[Export]
	private readonly float deceleration = 10f;
	[Export]
	private readonly DropData[] drops = new DropData[0];

	// private
	private Vector2 direction;
	private Vector2 damagePosition;
	private readonly PackedScene itemPickupScene = GD.Load<PackedScene>("res://Items/itemPickup/ItemPickup.tscn");

	// methods
	public override void Init()
	{
		Enemy.Connect(nameof(Enemy.EnemyDestroyed), this, nameof(OnEnemyDestroyed));
	}

	public override void Enter()
	{
		direction = Enemy.GlobalPosition.DirectionTo(damagePosition);
		Enemy.SetDirection(direction);
		Enemy.Velocity = direction * (-knockbackSpeed);
		Enemy.UpdateAnimation("destroy");
		Enemy.AnimationPlayer.Connect("animation_finished", this, nameof(OnAnimationPlayerAnimationFinished));
		Enemy.Invulnerable = true;
		DropItems();
		GlobalPlayerManager.Instance.Player.UpdateXP(Enemy.RewardXp);
	}

	public override void Exit()
	{
		Enemy.Invulnerable = false;
	}

	public override EnemyState Process(float delta)
	{
		Enemy.Velocity *= 1 - deceleration * delta;

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
				ItemPickup itemPickup = (ItemPickup)itemPickupScene.Instance();
				itemPickup.Item = drop.item;
				itemPickup.GlobalPosition = Enemy.GlobalPosition;
				itemPickup.Velocity = new Vector2(2, 2).Rotated((float)GD.RandRange(-1.5, 1.5)) * (float)GD.RandRange(0.9, 1.5);
				itemPickup.GetNode<AnimationPlayer>("AnimationPlayer").Play("default");
				Enemy.GetParent().CallDeferred("add_child", itemPickup);
			}
		}
	}
}
