using Godot;

public class DamageText : Node2D
{
    // private
    private Vector2 travelDistance = new Vector2(10, -20);

    // methods
    public void Start(string text, Vector2 position)
    {
        GetNode<Label>("Label").Text = text;
        GlobalPosition = position;

        travelDistance.y *= (float)GD.RandRange(0.5, 1.5);
        travelDistance.x *= (float)GD.RandRange(-1.5, 1.5);

        float duration = (float)GD.RandRange(0.75, 1.25);
        SceneTreeTween tween = CreateTween().SetParallel(true);
        tween.SetEase(Tween.EaseType.Out);
        tween.SetTrans(Tween.TransitionType.Quad);
        tween.TweenProperty(this, "global_position", GlobalPosition + travelDistance, duration);
        tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 0), duration);
        tween.Chain().TweenCallback(this, "queue_free");
    }
}
