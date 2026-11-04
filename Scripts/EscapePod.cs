#nullable enable

using Godot;

namespace Nave9_Evacuation.Scripts;

/// <summary>
/// Represents the escape pod vessel and extraction zone.
/// Activates terminal emissive visuals when power is restored and triggers
/// the victory scene when the player enters with all required batteries.
/// </summary>
public partial class EscapePod : Area3D
{
    private MeshInstance3D _screen = null!;
    private OmniLight3D _light = null!;

    /// <summary>
    /// Initializes visual node references and subscribes to body entry collision events.
    /// </summary>
    public override void _Ready()
    {
        _screen = GetNode<MeshInstance3D>("ScreenDisplay");
        _light = GetNode<OmniLight3D>("OmniLight3D");
        BodyEntered += OnBodyEntered;
    }

    /// <summary>
    /// Unlocks the pod console by updating display material and ambient light
    /// to an emissive green, signaling to the player that evacuation is now ready.
    /// </summary>
    public void UnlockConsole()
    {
        var greenMat = new StandardMaterial3D
        {
            AlbedoColor = new Color("#00E676"),
            EmissionEnabled = true,
            Emission = new Color("#00E676"),
            EmissionEnergyMultiplier = 4.0f
        };

        _screen.MaterialOverride = greenMat;
        _light.LightColor = new Color("#00E676");
    }

    /// <summary>
    /// Triggered when an entity enters the escape pod trigger boundary.
    /// Checks if the entrant is the player and has collected all required batteries to win.
    /// </summary>
    /// <param name="body">The 3D body entering the area.</param>
    private void OnBodyEntered(Node3D body)
    {
        if (body is Player player && player.BatteriesCollected >= Player.TotalBatteries)
        {
            // Restore mouse cursor visibility for menus
            Input.MouseMode = Input.MouseModeEnum.Visible;
            GetTree().ChangeSceneToFile("res://Scenes/Victory.tscn");
        }
    }
}
