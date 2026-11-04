#nullable enable

using Godot;

namespace Nave9_Evacuation.Scripts;

/// <summary>
/// Controller for the Victory / Mission Accomplished screen.
/// Shown when the player successfully powers and boards the escape pod.
/// </summary>
public partial class Victory : Control
{
    /// <summary>
    /// Restores cursor visibility and connects victory screen buttons.
    /// </summary>
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;

        var playAgainButton = GetNode<Button>("%PlayAgainButton");
        var menuButton = GetNode<Button>("%MenuButton");

        playAgainButton.Pressed += OnPlayAgainPressed;
        menuButton.Pressed += OnMenuPressed;
    }

    /// <summary>
    /// Restarts a new gameplay session with a newly generated maze.
    /// </summary>
    private void OnPlayAgainPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
    }

    /// <summary>
    /// Returns to the main menu screen.
    /// </summary>
    private void OnMenuPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Menu.tscn");
    }
}
