#nullable enable

using Godot;

namespace Nave9_Evacuation.Scripts;

/// <summary>
/// Controller for the Game Over screen.
/// Displays failure state when oxygen is exhausted and handles restart or menu navigation.
/// </summary>
public partial class GameOver : Control
{
    /// <summary>
    /// Restores cursor visibility and connects UI button click events.
    /// </summary>
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;

        var retryButton = GetNode<Button>("%RetryButton");
        var menuButton = GetNode<Button>("%MenuButton");

        retryButton.Pressed += OnRetryPressed;
        menuButton.Pressed += OnMenuPressed;
    }

    /// <summary>
    /// Restarts the main gameplay scene.
    /// </summary>
    private void OnRetryPressed()
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
