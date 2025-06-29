using Godot;

[Tool]
public class ItemPickup : KinematicBody2D
{
	// Signals
	[Signal]
	public delegate void PickedUp();

	// Exports
	[Export]
	public Items Item
	{
		get => item;
		set
		{
			item = value;

			if (Engine.EditorHint)
				UpdateTexture();
		}
	}

	// private
	private Items item;
	private Sprite sprite;
	private AudioStreamPlayer2D audioStreamPlayer2D;
	private Area2D area2D;

	// properties
	public Vector2 Velocity { get; set; }
	public bool DontPickup { get; private set; } = false;

	// methods
	public override void _Ready()
	{
		sprite = GetNode<Sprite>("Sprite");
		audioStreamPlayer2D = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
		area2D = GetNode<Area2D>("Area2D");

		UpdateTexture();

		if (Engine.EditorHint)
			return;

		area2D.Connect("body_entered", this, nameof(OnArea2DBodyEntered));
	}

	public override void _PhysicsProcess(float delta)
	{
		KinematicCollision2D collisionInfo = MoveAndCollide(Velocity);

		if (collisionInfo != null)
			Velocity = Velocity.Bounce(collisionInfo.Normal);

		Velocity *= 1 - delta * 4;
	}

	private void UpdateTexture()
	{
		if (sprite != null)
			sprite.Texture = Item.Texture;
	}

	private void ItemPickedUp()
	{
		audioStreamPlayer2D.Play();
		Hide();
		EmitSignal(nameof(PickedUp));
		audioStreamPlayer2D.Connect("finished", this, nameof(ItemPickedUp2));
	}

	private void ItemPickedUp2()
	{
		QueueFree();
	}

	private void OnArea2DBodyEntered(object body)
	{
		if (!(body is Player))
			return;

		if (GlobalPlayerManager.Instance.PlayerInventory.AddItem(Item))
			ItemPickedUp();
		else
			DontPickup = true;
	}
}
