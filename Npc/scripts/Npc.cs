using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(Npc), "res://Npc/icons/npc.png", nameof(KinematicBody2D))]
public class Npc : KinematicBody2D
{
    // Signals
    [Signal]
    public delegate void DoBehaviorEnabled();

    // Exports
    [Export]
    public NpcResource NpcResource
    {
        get => npcResource;
        set
        {
            npcResource = value;

            if (Engine.EditorHint)
                UpdateTexture();
        }
    }

    // private
    private NpcResource npcResource;
    private string directionName = "Down";
    private Sprite sprite;
    private AnimationPlayer animationPlayer;
    private Vector2 storeDirection;
    private string storeState;

    // properties
    public Vector2 Direction { get; set; } = Vector2.Down;
    public bool DoBehavior { get; private set; } = true;
    public string State { get; set; } = "idle";
    public Vector2 Velocity { get; set; } = Vector2.Zero;

    // methods
    public override void _Ready()
    {
        sprite = GetNode<Sprite>("Sprite");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        UpdateTexture();

        if (Engine.EditorHint)
            return;

        GatherInteractables(this);
        EmitSignal(nameof(DoBehaviorEnabled));
    }

    public override void _PhysicsProcess(float delta)
    {
        MoveAndSlide(Velocity);
    }

    private void UpdateTexture()
    {
        if (sprite != null)
            sprite.Texture = npcResource.Sprite;
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
            if (Direction.x < 0)
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
                dialogInteraction.Connect(nameof(DialogInteraction.PlayerInteracted), this, nameof(OnPlayerInteracted));
                dialogInteraction.Connect(nameof(DialogInteraction.Finished), this, nameof(OnFinished));
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
        EmitSignal(nameof(DoBehaviorEnabled));
    }
}
