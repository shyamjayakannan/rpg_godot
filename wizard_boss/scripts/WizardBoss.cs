using System;
using System.Collections.Generic;
using Godot;

public class WizardBoss : Node2D
{
    // Signals
    [Signal]
    private delegate void DarkWizardDefeated();

    // Exports
    [Export]
    private int maxHp = 10;
    [Export]
    private AudioStream hurtSound;
    [Export]
    private float teleportDelay = 1.5f;

    // private
    private int hp = 10;
    private int currentPosition;
    private List<Vector2> positions = new List<Vector2>();
    private List<EnergyBeam> beams = new List<EnergyBeam>();
    private Node2D positionTargets;
    private Node2D beamAttacks;
    private Node2D bossNode;
    private AnimationPlayer animationPlayer;
    private AnimationPlayer cloakAnimationPlayer;
    private AudioStreamPlayer2D audioStreamPlayer2D;
    private HitBox hitBox;
    private HurtBox hurtBox;
    private PackedScene explosionScene = GD.Load<PackedScene>("res://wizard_boss/EnergyExplosion.tscn");
    private PackedScene energyOrbScene = GD.Load<PackedScene>("res://wizard_boss/EnergyOrb.tscn");
    private Sprite Hand1;
    private Sprite Hand2;
    private int damageCounter = 0;
    private PersistentDataHandler persistentDataHandler;

    // methods
    public override void _Ready()
    {
        positionTargets = GetNode<Node2D>("PositionTargets");
        beamAttacks = GetNode<Node2D>("BeamAttacks");
        bossNode = GetNode<Node2D>("BossNode");
        animationPlayer = bossNode.GetNode<AnimationPlayer>("AnimationPlayer");
        cloakAnimationPlayer = bossNode.GetNode<AnimationPlayer>("Cloak/AnimationPlayer");
        audioStreamPlayer2D = bossNode.GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        hitBox = bossNode.GetNode<HitBox>("HitBox");
        hurtBox = bossNode.GetNode<HurtBox>("HurtBox");
        Hand1 = bossNode.GetNode<Sprite>("Cloak/Hand1_Down");
        Hand2 = bossNode.GetNode<Sprite>("Cloak/Hand2_Down");
        persistentDataHandler = GetNode<PersistentDataHandler>("PersistentDataHandler");

        persistentDataHandler.Connect(nameof(PersistentDataHandler.DataLoaded), this, nameof(SetState));
        persistentDataHandler.GetValue();

        hitBox.Connect(nameof(HitBox.Damaged), this, nameof(OnHitBoxDamaged));

        hp = maxHp;

        foreach (Sprite sprite in positionTargets.GetChildren())
            positions.Add(sprite.GlobalPosition);

        foreach (EnergyBeam child in beamAttacks.GetChildren())
            beams.Add(child);

        positionTargets.Hide();
        PlayerHUD.Instance.ShowBossHealthBar("DarkWizard");
        PlayerHUD.Instance.UpdateBossHealthBar(hp, maxHp);
        Teleport((int)(GD.Randi() % positions.Count));
    }

    private void SetState(bool alreadyDestroyed)
    {
        if (alreadyDestroyed)
        {
            QueueFree();
            GlobalSignalManager.Instance.EmitSignal(nameof(GlobalSignalManager.EnemiesDestroyed), true);
            return;
        }
    }

    private void BeamAttack()
    {
        EnergyBeam[] activeBeams;
        int halfCount = beams.Count / 2;
        int randomIndex = (int)(GD.Randi() % halfCount);

        if (currentPosition == 0 || currentPosition == 2)
            activeBeams = hp < 3 ? new EnergyBeam[3] { beams[0], beams[1], beams[2] } : new EnergyBeam[2] { beams[randomIndex], beams[(randomIndex + 1) % halfCount] };
        else
            activeBeams = hp < 3 ? new EnergyBeam[3] { beams[3], beams[4], beams[5] } : new EnergyBeam[2] { beams[randomIndex + halfCount], beams[(randomIndex + 1) % halfCount + halfCount] };

        foreach (EnergyBeam beam in activeBeams)
            beam.Attack();
    }

    private void OrbAttack()
    {
        EnergyOrb orb = (EnergyOrb)energyOrbScene.Instance();
        orb.GlobalPosition = bossNode.GlobalPosition + new Vector2(0, -34);
        GetParent().CallDeferred("add_child", orb);
    }

    private void Teleport(int location)
    {
        if (hp < 1)
            return;

        animationPlayer.Play("disappear");
        SetBoxes(false);
        damageCounter = 0;

        if (hp < 7)
            OrbAttack();

        GetTree().CreateTimer(teleportDelay, false).Connect("timeout", this, nameof(Teleport2), new Godot.Collections.Array(location));
    }

    private void Teleport2(int location)
    {
        if (hp < 1)
            return;

        currentPosition = location;
        bossNode.GlobalPosition = positions[location];
        UpdateAnimation();
        animationPlayer.Play("appear");
        GetTree().CreateTimer(animationPlayer.CurrentAnimationLength, false).Connect("timeout", this, nameof(Idle));
    }

    private void Idle()
    {
        if (hp < 1)
            return;

        SetBoxes(true);
        animationPlayer.Play("idle");
        GetTree().CreateTimer(teleportDelay, false).Connect("timeout", this, nameof(Idle2));
    }

    private void Idle2()
    {
        if (hp < 1)
            return;

        if (damageCounter < 1)
        {
            animationPlayer.Play("castSpell");
            BeamAttack();
            GetTree().CreateTimer(animationPlayer.CurrentAnimationLength, false).Connect("timeout", this, nameof(Teleport), new Godot.Collections.Array((int)(GD.Randi() % positions.Count)));
            return;
        }

        Teleport((int)(GD.Randi() % positions.Count));
    }

    private void UpdateAnimation()
    {
        bossNode.Scale = new Vector2(1, 1);
        Hand2.FlipH = true;

        switch (currentPosition)
        {
            case 1:
                cloakAnimationPlayer.Play("down");
                SetHandRegion("down");
                break;

            case 3:
                cloakAnimationPlayer.Play("up");
                SetHandRegion("up");
                break;

            default:
                if (currentPosition == 2)
                    bossNode.Scale = new Vector2(-1, 1);

                Hand2.FlipH = false;
                SetHandRegion("side");
                cloakAnimationPlayer.Play("side");
                break;
        }
    }

    private void SetHandRegion(string direction)
    {
        float x1 = 0;
        float x2 = 0;

        switch (direction)
        {
            case "up":
                x1 = 128;
                x2 = 128;
                break;

            case "side":
                x1 = 256;
                x2 = 384;
                break;
        }

        Hand1.RegionRect = new Rect2(x1, 0, Hand1.RegionRect.Size.x, Hand1.RegionRect.Size.y);
        Hand2.RegionRect = new Rect2(x2, 0, Hand2.RegionRect.Size.x, Hand2.RegionRect.Size.y);
    }

    private void OnHitBoxDamaged(HurtBox @hurtBox)
    {
        if (animationPlayer.CurrentAnimation == "damaged")
            return;

        damageCounter++;
        hp = Mathf.Clamp(hp - hurtBox.Damage, 0, maxHp);
        PlayerHUD.Instance.UpdateBossHealthBar(hp, maxHp);
        animationPlayer.Play("damaged");
        PlayAudio(hurtSound);

        if (hp < 1)
            Defeat();
    }

    private void Defeat()
    {
        beams.ForEach(beam => beam.Stop());
        animationPlayer.Play("destroy");
        SetBoxes(false);
        persistentDataHandler.SetValue();
        PlayerHUD.Instance.HideBossHealthBar();
        animationPlayer.Connect("animation_finished", this, nameof(Finish));
    }

    private void Finish(string animName)
    {
        EmitSignal(nameof(DarkWizardDefeated));
        GlobalSignalManager.Instance.EmitSignal(nameof(GlobalSignalManager.EnemiesDestroyed), false);
        QueueFree();
    }

    private void PlayAudio(AudioStream stream)
    {
        audioStreamPlayer2D.Stream = stream;
        audioStreamPlayer2D.Play();
    }

    private void SetBoxes(bool value)
    {
        hitBox.SetDeferred("monitoring", value);
        hurtBox.SetDeferred("monitorable", value);
    }

    // called in destroy animation track
    private void Explosion(Vector2 position)
    {
        Node2D explosion = (Node2D)explosionScene.Instance();
        explosion.GlobalPosition = bossNode.GlobalPosition + position;
        GetParent().AddChild(explosion);
    }
}
