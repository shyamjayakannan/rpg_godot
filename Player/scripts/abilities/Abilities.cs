using Godot;

public partial class Abilities : Node
{
    // private
    private enum PlayerAbilities
    {
        Boomerang,
        Grapple,
        Bow,
        Bomb
    }
    private PlayerAbilities selectedAbility = PlayerAbilities.Boomerang;
    private PackedScene boomerangScene = GD.Load<PackedScene>("res://Player/scripts/abilities/boomerang/Boomerang.tscn");
    private PackedScene bombScene = GD.Load<PackedScene>("res://Player/scripts/abilities/bomb/Bomb.tscn");
    private Boomerang boomerang;
    private IdleState idleState;
    private WalkState walkState;
    private LiftState liftState;
    private BowState bowState;

    // methods
    public override void _Ready()
    {
        idleState = GetNode<IdleState>("../PlayerStateMachine/IdleState");
        walkState = GetNode<WalkState>("../PlayerStateMachine/WalkState");
        liftState = GetNode<LiftState>("../PlayerStateMachine/LiftState");
        bowState = GetNode<BowState>("../PlayerStateMachine/BowState");
        PlayerHUD.Instance.UpdateArrows(GlobalPlayerManager.Instance.Player.Arrows);
        PlayerHUD.Instance.UpdateBombs(GlobalPlayerManager.Instance.Player.Bombs);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ability"))
        {
            switch (selectedAbility)
            {
                case PlayerAbilities.Boomerang:
                    BoomerangAbility();
                    break;

                case PlayerAbilities.Grapple:
                    GrappleAbility();
                    break;

                case PlayerAbilities.Bow:
                    BowAbility();
                    break;

                case PlayerAbilities.Bomb:
                    BombAbility();
                    break;
            }
        }
        else if (@event.IsActionPressed("switchAbility"))
        {
            int i = ((int)selectedAbility + 1) % 4;
            PlayerHUD.Instance.UpdateAbilityUI(i);
            selectedAbility = (PlayerAbilities)i;
        }
    }

    private void BombAbility()
    {
        Player player = GlobalPlayerManager.Instance.Player;
        State currentState = player.GetCurrentState();

        if (player.Bombs <= 0 || player.Throwable != null || (currentState != idleState && currentState != walkState))
            return;

        liftState.SetStartLate(true);
        PlayerHUD.Instance.UpdateBombs(--player.Bombs);
        Node2D bomb = (Node2D)bombScene.Instantiate();
        YSortHandler.AddToScene(bomb, player);
        bomb.GetNode<Bomb>("Throwable").OnInteractPressed();
    }

    private void BowAbility()
    {
        Player player = GlobalPlayerManager.Instance.Player;
        State currentState = player.GetCurrentState();

        if (player.Arrows <= 0 || (currentState != idleState && currentState != walkState))
            return;

        PlayerHUD.Instance.UpdateArrows(--player.Arrows);
        player.SetCurrentState(bowState);
    }

    private static void GrappleAbility()
    {
    }

    private void BoomerangAbility()
    {
        Player player = GlobalPlayerManager.Instance.Player;

        if (boomerang == null)
        {
            boomerang = (Boomerang)boomerangScene.Instantiate();
            YSortHandler.AddToScene(boomerang, player);
        }

        if (boomerang.BoomerangState != Boomerang.State.INACTIVE)
            return;

        boomerang.GlobalPosition = GlobalPlayerManager.Instance.Player.GlobalPosition;

        if (player.Direction == Vector2.Zero)
            boomerang.Throw(player.CardinalDirection);
        else
            boomerang.Throw(player.Direction);
    }
}
