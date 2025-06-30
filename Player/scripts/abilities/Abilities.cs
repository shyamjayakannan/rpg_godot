using Godot;

public class Abilities : Node
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
    private readonly PackedScene boomerangScene = GD.Load<PackedScene>("res://Player/Boomerang.tscn");
    private readonly PackedScene bombScene = GD.Load<PackedScene>("res://Interactables/bomb/Bomb.tscn");
    private Boomerang boomerang;

    // methods

    public override void _Ready()
    {
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

        if (player.Bombs <= 0)
            return;

        PlayerHUD.Instance.UpdateBombs(--player.Bombs);
    }

    private void BowAbility()
    {
    }

    private void GrappleAbility()
    {
    }

    private void BoomerangAbility()
    {
        Player player = GlobalPlayerManager.Instance.Player;

        if (boomerang == null)
        {
            boomerang = (Boomerang)boomerangScene.Instance();
            player.AddChild(boomerang);
        }

        if (boomerang.BoomerangState != Boomerang.State.INACTIVE)
            return;

        if (player.Direction == Vector2.Zero)
            boomerang.Throw(player.CardinalDirection);
        else
            boomerang.Throw(player.Direction);
    }
}
