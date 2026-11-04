#nullable enable

using Godot;

namespace Nave9_Evacuation.Scripts;

/// <summary>
/// Controller for the main title menu.
/// Manages initial game launch and application exit requests.
/// </summary>
public partial class Menu : Control
{
    /// <summary>
    /// Restores cursor visibility and binds click events to menu action buttons.
    /// </summary>
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;

        var startButton = GetNode<Button>("%StartButton");
        var quitButton = GetNode<Button>("%QuitButton");

        startButton.Pressed += OnStartPressed;
        quitButton.Pressed += OnQuitPressed;
    }

    /// <summary>
    /// Loads and transitions to the main gameplay scene.
    /// </summary>
    private void OnStartPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
    }

    /// <summary>
    /// Terminates the application.
    /// </summary>
    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
