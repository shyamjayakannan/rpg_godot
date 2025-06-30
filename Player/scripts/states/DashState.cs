using Godot;

public class DashState : State
{
    // Exports
    [Export]
    private readonly float moveSpeed = 250;
    [Export]
    private readonly float effectDelay = 0.05f;
    [Export]
    private readonly AudioStream dashSound;

    // private
    private State idleState;
    private State nextState = null;
    private Vector2 direction = Vector2.Zero;
    private float effectTimer = 0;
    private AudioStreamPlayer2D audioStreamPlayer2D;

    // methods
    public override void _Ready()
    {
        idleState = GetNode<IdleState>("../IdleState");
        audioStreamPlayer2D = GetNode<AudioStreamPlayer2D>("../../Audio/AttackSound");
    }

    public override void Enter()
    {
        Player.UpdateAnimation("dash");
        Player.MakeInvulnerable(Player.AnimationPlayer.CurrentAnimationLength);
        direction = Player.Direction == Vector2.Zero ? Player.CardinalDirection : Player.Direction;
        audioStreamPlayer2D.Stream = dashSound;
        audioStreamPlayer2D.Play();
        effectTimer = effectDelay;

        GetTree().CreateTimer(Player.AnimationPlayer.CurrentAnimationLength).Connect("timeout", this, nameof(OnAnimationFinished));
    }

    public override State Process(float delta)
    {
        Player.Velocity = moveSpeed * direction;
        effectTimer += delta;

        if (effectTimer >= effectDelay)
        {
            SpawnEffect();
            effectTimer = 0;
        }

        return nextState;
    }

    public override void Exit()
    {
        nextState = null;
    }

    public override State HandleInput(InputEvent _event)
    {
        if (_event.IsActionPressed("attack"))
            Player.UpdateAnimation("charge");

        return null;
    }

    private void OnAnimationFinished()
    {
        nextState = idleState;
    }

    private void SpawnEffect()
    {
        Node2D effect = new Node2D();
        Player.GetParent().AddChild(effect);
        effect.GlobalPosition = Player.GlobalPosition - new Vector2(0, 0.1f);
        Sprite spriteCopy = (Sprite)Player.Sprite.Duplicate();
        effect.AddChild(spriteCopy);
        SceneTreeTween sceneTreeTween = CreateTween();
        sceneTreeTween.SetEase(Tween.EaseType.Out);
        sceneTreeTween.TweenProperty(effect, "modulate", new Color(1, 1, 1, 0), 0.2f);
        sceneTreeTween.Chain().TweenCallback(effect, "queue_free");
    }
}
