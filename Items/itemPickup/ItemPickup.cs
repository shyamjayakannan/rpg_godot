using System;
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
	private PersistentDataHandler persistentDataHandler;
	private bool pickedUp = false;

	// properties
	public Vector2 Velocity { get; set; } = Vector2.Zero;
	public bool DontPickup { get; private set; } = false;
	public bool IsDroppedItem { get; set; } = false;
	public Vector2 SavedPosition { get; set; }

	// methods
	public override void _Ready()
	{
		sprite = GetNode<Sprite>("Sprite");
		audioStreamPlayer2D = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
		area2D = GetNode<Area2D>("Area2D");

		UpdateTexture();

		if (Engine.EditorHint)
			return;

		// VERY IMPORTANT
		// since persistentdata handler isnt a tool script, it wont be recognized in the editor and we anyway dont need it
		// when code runs in editor so thats why declaring it here. otherwise, we have to make it a tool script too.
		persistentDataHandler = GetNode<PersistentDataHandler>("PersistentDataHandler");

		area2D.Connect("body_entered", this, nameof(OnArea2DBodyEntered));
		persistentDataHandler.Connect(nameof(PersistentDataHandler.DataLoaded), this, nameof(SetState));
		persistentDataHandler.GetValue();
	}

	private void SetState(bool value)
	{
		if (value)
			QueueFree();
	}

	public override void _PhysicsProcess(float delta)
	{
		KinematicCollision2D collisionInfo = MoveAndCollide(Velocity);

		if (collisionInfo != null)
			Velocity = Velocity.Bounce(collisionInfo.Normal);

		Velocity *= 1 - delta * 4;

	}

	public override void _ExitTree()
	{
		if (IsDroppedItem && !pickedUp)
			GlobalLevelManager.Instance.AddItem(GetTree().CurrentScene.Filename, Item, GlobalPosition);
	}

	private void UpdateTexture()
	{
		if (sprite != null)
			sprite.Texture = Item.Texture;
	}

	private void ItemPickedUp()
	{
		pickedUp = true;
		audioStreamPlayer2D.Play();
		Hide();
		EmitSignal(nameof(PickedUp));
		audioStreamPlayer2D.Connect("finished", this, nameof(ItemPickedUp2));
	}

	private void ItemPickedUp2()
	{
		persistentDataHandler.SetValue();
		QueueFree();

		if (IsDroppedItem)
			GlobalLevelManager.Instance.RemoveItem(GetTree().CurrentScene.Filename, Item, SavedPosition);
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
