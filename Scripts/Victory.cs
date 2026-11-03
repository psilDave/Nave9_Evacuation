using Godot;

namespace Nave9_Evacuation.Scripts;

public partial class Victory : Control
{
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;

        var playAgainButton = GetNode<Button>("%PlayAgainButton");
        var menuButton = GetNode<Button>("%MenuButton");

        playAgainButton.Pressed += OnPlayAgainPressed;
        menuButton.Pressed += OnMenuPressed;
    }

    private void OnPlayAgainPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
    }

    private void OnMenuPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Menu.tscn");
    }
}