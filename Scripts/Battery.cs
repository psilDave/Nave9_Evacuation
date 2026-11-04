#nullable enable

using Godot;

namespace Nave9_Evacuation.Scripts;

/// <summary>
/// Represents a collectible power battery item in the game world.
/// Rotates around its vertical axis to provide visual feedback and self-destructs upon collection.
/// </summary>
public partial class Battery : StaticBody3D
{
    /// <summary>
    /// Display name of the item used in UI prompts or inventory references.
    /// </summary>
    [Export] public string ItemName { get; set; } = "Bateria de Lítio-Titânio";

    /// <summary>
    /// Called when the player collects this battery.
    /// Removes the battery node from the scene tree.
    /// </summary>
    public void Collect()
    {
        QueueFree();
    }

    /// <summary>
    /// Frame-by-frame physics/rendering update.
    /// Continuously rotates the battery mesh around the Y-axis.
    /// </summary>
    /// <param name="delta">Elapsed time in seconds since the previous frame.</param>
    public override void _Process(double delta)
    {
        RotateY(1.5f * (float)delta);
    }
}
