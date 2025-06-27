using Godot;

public class ChargeState : State
{
    // Export
    [Export]
    private readonly float chargeDuration = 1;
    [Export]
    private readonly float moveSpeed = 80;
    [Export]
    private readonly AudioStream sfxCharged;
    [Export]
    private readonly AudioStream sfxSpin;

    // private
    private IdleState idleState;
    private bool isAttacking = false;
    private bool walking = false;
    private Particles2D particles;
    private ParticlesMaterial particlesMaterial;
    private Timer timer;
    private HurtBox hurtBox;
    private AudioStreamPlayer2D audioStreamPlayer2D;
    private AnimationPlayer animationPlayer;

    // methods
    public override void _Ready()
    {
        idleState = GetNode<IdleState>("../IdleState");
        timer = GetNode<Timer>("Timer");
        hurtBox = GetNode<HurtBox>("../../Sprite/ChargeHurtBox");
        audioStreamPlayer2D = GetNode<AudioStreamPlayer2D>("../../Audio/AttackSound");
        animationPlayer = GetNode<AnimationPlayer>("../../Sprite/SpinSprite/AnimationPlayer");
        particles = GetNode<Particles2D>("../../Sprite/ChargeHurtBox/Particles2D");
        particlesMaterial = (ParticlesMaterial)particles.ProcessMaterial;

        particles.Emitting = false;
        timer.Connect("timeout", this, nameof(OnTimerTimeout));
    }

    public override void Enter()
    {
        particles.Emitting = true;
        timer.WaitTime = chargeDuration;
        timer.Start();
        isAttacking = false;
        walking = false;
        hurtBox.Monitorable = false;

        particles.Amount = 8;
        particles.Explosiveness = 0;
        particlesMaterial.InitialVelocity = 50;
    }

    public override void Exit()
    {
        timer.Stop();
        particles.Emitting = false;
        hurtBox.SetDeferred("monitorable", false);
    }

    public override State Process(float delta)
    {
        if (!isAttacking)
        {
            if (Player.Direction == Vector2.Zero)
            {
                walking = false;
                Player.UpdateAnimation("charge");
            }
            else if (Player.SetDirection() || !walking)
            {
                walking = true;
                Player.UpdateAnimation("chargeWalk");
            }
        }

        Player.Velocity = Player.Direction * moveSpeed;
        return null;
    }

    public override State HandleInput(InputEvent _event)
    {
        if (_event.IsActionReleased("attack"))
        {
            if (timer.TimeLeft > 0)
                return idleState;
            else if (!isAttacking)
                ChargeAttack();
        }

        return null;
    }

    private void ChargeAttack()
    {
        hurtBox.Monitorable = true;
        isAttacking = true;
        Player.AnimationPlayer.Play("chargeAttack");
        animationPlayer.Play("spin");
        Player.AnimationPlayer.Seek(GetSpinFrame());
        PlayAudio(sfxSpin);

        float duration = Player.AnimationPlayer.CurrentAnimationLength;
        Player.MakeInvulnerable(duration);

        GetTree().CreateTimer(duration * 0.875f, false).Connect("timeout", this, nameof(ChargeAttack2));
    }

    private void ChargeAttack2()
    {
        StateMachine.ChangeState(idleState);
    }

    private float GetSpinFrame()
    {
        float interval = 0.05f;

        if (Player.CardinalDirection == Vector2.Down)
            return interval * 0;
        else if (Player.CardinalDirection == Vector2.Up)
            return interval * 4;

        return interval * 6;
    }

    private void PlayAudio(AudioStream stream)
    {
        audioStreamPlayer2D.Stream = stream;
        audioStreamPlayer2D.Play();
    }

    private void OnTimerTimeout()
    {
        PlayAudio(sfxCharged);
        particles.Amount = 100;
        particles.Explosiveness = 1;
        particlesMaterial.InitialVelocity = 100;
    }
}
