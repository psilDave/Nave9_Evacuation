using Godot;

namespace Nave9_Evacuation.Scripts;

public partial class EscapePod : Area3D
{
    private MeshInstance3D _screen = null!;
    private OmniLight3D _light = null!;

    public override void _Ready()
    {
        _screen = GetNode<MeshInstance3D>("ScreenDisplay");
        _light = GetNode<OmniLight3D>("OmniLight3D");
        BodyEntered += OnBodyEntered;
    }

    public void UnlockConsole()
    {
        var greenMat = new StandardMaterial3D();
        greenMat.AlbedoColor = new Color("#00E676");
        greenMat.EmissionEnabled = true;
        greenMat.Emission = new Color("#00E676");
        greenMat.EmissionEnergyMultiplier = 4.0f;

        _screen.MaterialOverride = greenMat;
        _light.LightColor = new Color("#00E676");
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is Player player && player.BatteriesCollected >= Player.TotalBatteries)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            GetTree().ChangeSceneToFile("res://Scenes/Victory.tscn");
        }
    }
}