using Godot;

namespace Nave9_Evacuation.Scripts;

public partial class GameOver : Control
{
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;

        var retryButton = GetNode<Button>("%RetryButton");
        var menuButton = GetNode<Button>("%MenuButton");

        retryButton.Pressed += OnRetryPressed;
        menuButton.Pressed += OnMenuPressed;
    }

    private void OnRetryPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
    }

    private void OnMenuPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Menu.tscn");
    }
}