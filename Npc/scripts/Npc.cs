using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(Npc), "res://Npc/icons/npc.png", nameof(CharacterBody2D))]
public partial class Npc : CharacterBody2D
{
    // Signals
    [Signal]
    public delegate void DoBehaviorEnabledEventHandler();

    // Exports
    [Export]
    public NpcResource NpcResource
    {
        get => npcResource;
        set
        {
            npcResource = value;

            if (Engine.IsEditorHint())
                UpdateTexture();
        }
    }

    // private
    private NpcResource npcResource;
    private string directionName = "Down";
    private Sprite2D sprite;
    private AnimationPlayer animationPlayer;
    private Vector2 storeDirection;
    private string storeState;

    // properties
    public Vector2 Direction { get; set; } = Vector2.Down;
    public bool DoBehavior { get; private set; } = true;
    public string State { get; set; } = "idle";

    // methods
    public override void _Ready()
    {
        sprite = GetNode<Sprite2D>("Sprite2D");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        UpdateTexture();

        if (Engine.IsEditorHint())
            return;

        GatherInteractables(this);
        EmitSignal(SignalName.DoBehaviorEnabled);
    }

    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();
    }

    private void UpdateTexture()
    {
        if (sprite != null)
            sprite.Texture = npcResource.Sprite2D;
    }

    public void UpdateAnimation()
    {
        animationPlayer.Play($"{State}{directionName}");
    }

    public void UpdateDirection(Vector2 newDirection)
    {
        Direction = newDirection;
        float angle = Direction.Angle();

        if (angle >= Mathf.Pi / 4 && angle < 3 * Mathf.Pi / 4)
            directionName = "Down";
        else if (angle >= -3 * Mathf.Pi / 4 && angle < -Mathf.Pi / 4)
            directionName = "Up";
        else
        {
            if (Direction.X < 0)
                sprite.Scale = new Vector2(-1, 1);
            else
                sprite.Scale = new Vector2(1, 1);

            directionName = "Side";
        }
    }

    private void GatherInteractables(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is DialogInteraction dialogInteraction)
            {
                dialogInteraction.Connect(DialogInteraction.SignalName.PlayerInteracted, new(this, MethodName.OnPlayerInteracted));
                dialogInteraction.Connect(DialogInteraction.SignalName.Finished, new(this, MethodName.OnFinished));
            }

            GatherInteractables(child);
        }
    }

    private void OnPlayerInteracted()
    {
        storeDirection = Direction;
        storeState = State;
        State = "idle";
        UpdateDirection(GlobalPosition.DirectionTo(GlobalPlayerManager.Instance.Player.GlobalPosition));
        UpdateAnimation();
        DoBehavior = false;
    }

    private void OnFinished()
    {
        State = storeState;
        UpdateDirection(storeDirection);
        UpdateAnimation();
        DoBehavior = true;
        EmitSignal(SignalName.DoBehaviorEnabled);
    }
}
