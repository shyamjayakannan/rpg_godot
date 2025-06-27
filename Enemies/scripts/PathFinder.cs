using System.Collections.Generic;
using System.Linq;
using Godot;

public class PathFinder : Node2D
{
    // private
    private Vector2[] vectors = new Vector2[16]{
        Vector2.Up,
        new Vector2(1, -2).Normalized(),
        new Vector2(1, -1).Normalized(),
        new Vector2(2, -1).Normalized(),
        Vector2.Right,
        new Vector2(2, 1).Normalized(),
        new Vector2(1, 1).Normalized(),
        new Vector2(1, 2).Normalized(),
        Vector2.Down,
        new Vector2(-1, 2).Normalized(),
        new Vector2(-1, 1).Normalized(),
        new Vector2(-2, 1).Normalized(),
        Vector2.Left,
        new Vector2(-2, -1).Normalized(),
        new Vector2(-1, -1).Normalized(),
        new Vector2(-1, -2).Normalized()
    };
    private float[] interests = new float[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    private float[] obstacles = new float[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    private List<RayCast2D> rayCasts = new List<RayCast2D>(16);
    private Timer timer;

    // properties
    public Vector2 BestPath { get; private set; } = Vector2.Zero;

    // methods
    public override void _Ready()
    {
        timer = GetNode<Timer>("Timer");
        timer.Connect("timeout", this, nameof(SetPath));
        Node parent = GetParent();

        foreach (Node child in GetChildren())
        {
            if (child is RayCast2D rayCast)
            {
                rayCast.Enabled = true;
                rayCast.AddException(parent);
                rayCasts.Add(rayCast);
            }
        }
    }

    private void SetPath()
    {
        Vector2 playerDirection = GlobalPosition.DirectionTo(GlobalPlayerManager.Instance.Player.GlobalPosition);

        // VERY IMPORTANT
        // we need all to be 0 before continuing. if we set this inside the below loop, some may not be 0 initially
        // this is possible after the first call to setpath when the array already has some values
        for (int i = 0; i < 16; i++)
            obstacles[i] = 0;

        for (int i = 0; i < 16; i++)
        {
            if (!rayCasts[i].IsColliding())
                continue;

            obstacles[i] += 4;
            obstacles[(i + 1) % 16] += 2;
            obstacles[(i + 15) % 16] += 2;
            obstacles[(i + 2) % 16] += 1;
            obstacles[(i + 14) % 16] += 1;
        }

        if (obstacles.Max() == 0)
        {
            BestPath = playerDirection;
            return;
        }

        for (int i = 0; i < 16; i++)
            interests[i] = vectors[i].Dot(playerDirection) - obstacles[i];


        BestPath = vectors[GetMaxValueIndex(interests)];
    }

    private int GetMaxValueIndex(float[] array)
    {
        float max = float.MinValue;
        int maxIndex = 0;

        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] > max)
            {
                max = array[i];
                maxIndex = i;
            }
        }
        // GD.Print(maxIndex, max);
        return maxIndex;
    }
}
