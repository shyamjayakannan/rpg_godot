using Godot;

public class GlobalEffectManager : Node
{
    // private
    private PackedScene damageTextScene = GD.Load<PackedScene>("res://AutoLoads/globalEffects/DamageText.tscn");

    // properties
    public static GlobalEffectManager Instance { get; private set; }

    // methods
    public override void _Ready()
    {
        Instance = this;
    }

    public void DamageTexter(string text, Vector2 position)
    {
        DamageText damageText = (DamageText)damageTextScene.Instance();
        AddChild(damageText);
        damageText.Start(text, position);
    }
}
