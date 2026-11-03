using Godot;

namespace Nave9_Evacuation.Scripts;

public partial class Menu : Control
{
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;

        var startButton = GetNode<Button>("%StartButton");
        var quitButton = GetNode<Button>("%QuitButton");

        startButton.Pressed += OnStartPressed;
        quitButton.Pressed += OnQuitPressed;
    }

    private void OnStartPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}