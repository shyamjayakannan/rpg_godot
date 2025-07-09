using Godot;

public partial class ChargeState : State
{
    // Export
    [Export]
    private float chargeDuration = 1;
    [Export]
    private float moveSpeed = 80;
    [Export]
    private AudioStream sfxCharged;
    [Export]
    private AudioStream sfxSpin;

    // private
    private IdleState idleState;
    private bool isAttacking = false;
    private bool walking = false;
    private GpuParticles2D particles;
    private ParticleProcessMaterial particlesMaterial;
    private Timer timer;
    private HurtBox hurtBox;
    private AudioStreamPlayer2D audioStreamPlayer2D;
    private AnimationPlayer animationPlayer;

    // methods
    public override void _Ready()
    {
        idleState = GetNode<IdleState>("../IdleState");
        timer = GetNode<Timer>("Timer");
        hurtBox = GetNode<HurtBox>("../../Sprite2D/ChargeHurtBox");
        audioStreamPlayer2D = GetNode<AudioStreamPlayer2D>("../../Audio/AttackSound");
        animationPlayer = GetNode<AnimationPlayer>("../../Sprite2D/SpinSprite/AnimationPlayer");
        particles = GetNode<GpuParticles2D>("../../Sprite2D/ChargeHurtBox/GPUParticles2D");
        particlesMaterial = (ParticleProcessMaterial)particles.ProcessMaterial;

        particles.Emitting = false;
        timer.Connect(Timer.SignalName.Timeout, new(this, MethodName.OnTimerTimeout));
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
        hurtBox.SetDeferred(Area2D.PropertyName.Monitorable, false);
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

        double duration = Player.AnimationPlayer.CurrentAnimationLength;
        Player.MakeInvulnerable((float)duration);

        GetTree().CreateTimer(duration * 0.875f, false).Connect(SceneTreeTimer.SignalName.Timeout, Callable.From(() => StateMachine.ChangeState(idleState)));
    }

    private static float GetSpinFrame()
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
        particlesMaterial.ini = 100;
    }
}
