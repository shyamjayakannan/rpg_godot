using System.Collections.Generic;
using Godot;

public class ItemMagnet : Area2D
{
    // Exports
    [Export]
    private readonly float magnetStrength = 100.0f;

    // private
    private readonly List<ItemPickup> itemPickups = new List<ItemPickup>();
    private readonly List<float> speeds = new List<float>();

    // methods
    public override void _Ready()
    {
        Connect("body_entered", this, nameof(OnItemMagnetBodyEntered));
    }

    public override void _Process(float delta)
    {
        bool allNull = true;

        for (int i = 0; i < itemPickups.Count; i++)
        {
            if (!IsInstanceValid(itemPickups[i]) || itemPickups[i].DontPickup)
                continue;

            allNull = false;

            if (itemPickups[i].GlobalPosition.DistanceTo(GlobalPosition) > 1)
            {
                speeds[i] += magnetStrength * delta;
                itemPickups[i].Position += itemPickups[i].GlobalPosition.DirectionTo(GlobalPosition) * speeds[i] * delta;
            }
            else
                itemPickups[i].Position = GlobalPosition;
        }

        if (allNull)
        {
            speeds.Clear();
            itemPickups.Clear();
        }
    }

    private void OnItemMagnetBodyEntered(Node2D body)
    {
        if (!(body is ItemPickup pickup) || (pickup.Item is EquipableItem equipableItem && GlobalPlayerManager.Instance.IsEquipmentPresent(equipableItem)))
            return;

        // we want the player to be able to pull items that may be on the other side of a collision object
        // so we disable its physics processing (see the physics process function of itempickup)
        body.SetPhysicsProcess(false);
        itemPickups.Add((ItemPickup)body);
        speeds.Add(magnetStrength);
    }
}
