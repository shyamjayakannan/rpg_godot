using Godot;

[Tool]
public partial class ItemPickup : CharacterBody2D
{
	// Signals
	[Signal]
	public delegate void PickedUpEventHandler();

	// Exports
	[Export]
	public Items Item
	{
		get => item;
		set
		{
			item = value;

			if (Engine.IsEditorHint())
				UpdateTexture();
		}
	}

	// private
	private Items item;
	private Sprite2D sprite;
	private AudioStreamPlayer2D audioStreamPlayer2D;
	private Area2D area2D;
	private PersistentDataHandler persistentDataHandler;
	private bool pickedUp = false;

	// properties
	public bool DontPickup { get; private set; } = false;
	public bool IsDroppedItem { get; set; } = false;
	public Vector2 SavedPosition { get; set; }

	// methods
	public override void _Ready()
	{
		sprite = GetNode<Sprite2D>("Sprite2D");
		audioStreamPlayer2D = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
		area2D = GetNode<Area2D>("Area2D");

		UpdateTexture();

		if (Engine.IsEditorHint())
			return;

		// VERY IMPORTANT
		// since persistentdata handler isnt a tool script, it wont be recognized in the editor and we anyway dont need it
		// when code runs in editor so thats why declaring it here. otherwise, we have to make it a tool script too.
		persistentDataHandler = GetNode<PersistentDataHandler>("PersistentDataHandler");

		area2D.Connect(Area2D.SignalName.BodyEntered, new(this, MethodName.OnArea2DBodyEntered));
		persistentDataHandler.Connect(PersistentDataHandler.SignalName.DataLoaded, new(this, MethodName.SetState));
		persistentDataHandler.GetValue();
	}

	private void SetState(bool value)
	{
		if (value)
			QueueFree();
	}

	public override void _PhysicsProcess(double delta)
	{
		KinematicCollision2D collisionInfo = MoveAndCollide(Velocity);

		if (collisionInfo != null)
			Velocity = Velocity.Bounce(collisionInfo.GetNormal());

		Velocity *= 1 - (float)delta * 4;

	}

	public override void _ExitTree()
	{
		if (IsDroppedItem && !pickedUp)
			GlobalLevelManager.Instance.AddItem(GetTree().CurrentScene.SceneFilePath, Item, GlobalPosition);
	}

	private void UpdateTexture()
	{
		if (sprite != null)
			sprite.Texture = Item?.Texture2D;
	}

	private void ItemPickedUp()
	{
		pickedUp = true;
		audioStreamPlayer2D.Play();
		Hide();
		EmitSignal(SignalName.PickedUp);
		audioStreamPlayer2D.Connect(AudioStreamPlayer2D.SignalName.Finished, new(this, MethodName.ItemPickedUp2));
	}

	private void ItemPickedUp2()
	{
		persistentDataHandler.SetValue();
		QueueFree();

		if (IsDroppedItem)
			GlobalLevelManager.Instance.RemoveItem(GetTree().CurrentScene.SceneFilePath, Item, SavedPosition);
	}

	private void OnArea2DBodyEntered(Node body)
	{
		if (body is not Player)
			return;

		if (GlobalPlayerManager.Instance.PlayerInventory.AddItem(Item))
			ItemPickedUp();
		else
			DontPickup = true;
	}
}
