using Godot;

public partial class Torch : PointLight2D
{
    // private
    private float time = 0.0f;

    // methods
    public override void _Process(double delta)
    {
        time += (float)delta;

        if (time > 0.2f)
        {
            Flicker();
            time = 0.0f;
        }
    }

    private void Flicker()
    {
        Energy = 0.9f + GD.Randf() * 0.1f;
        Scale = Vector2.One * Energy;
    }
}
