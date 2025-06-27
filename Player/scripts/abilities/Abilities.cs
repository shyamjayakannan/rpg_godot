using Godot;

public class Abilities : Node
{
    // private
    private enum PlayerAbilities
    {
        BOOMERANG,
        MAGNET
    }
    private readonly PlayerAbilities selectedAbility = PlayerAbilities.BOOMERANG;
    private readonly PackedScene boomerangScene = GD.Load<PackedScene>("res://Player/Boomerang.tscn");
    private Boomerang boomerang;

    // methods
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ability"))
            if (selectedAbility == PlayerAbilities.BOOMERANG)
                BoomerangAbility();
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
