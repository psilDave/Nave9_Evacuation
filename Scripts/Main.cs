using Godot;

namespace Nave9_Evacuation.Scripts;

public partial class Main : Node3D
{
    private Player _player = null!;
    private HUD _hud = null!;
    private MazeGenerator _mazeGen = null!;
    private Node3D _itemsContainer = null!;
    private EscapePod? _escapePod;

    public override void _Ready()
    {
        _player = GetNode<Player>("Player");
        _hud = GetNode<HUD>("HUD");
        _mazeGen = GetNode<MazeGenerator>("MazeGenerator");
        _itemsContainer = GetNode<Node3D>("Items");

        _mazeGen.OnEscapePodSpawned += OnPodReady;

        _player.OnOxygenChanged += _hud.UpdateOxygen;
        _player.OnBatteriesChanged += HandleBatteryProgress;

        _mazeGen.GenerateAndBuild(_player, _itemsContainer);

        _hud.UpdateOxygen(_player.MaxOxygen, _player.MaxOxygen);
        _hud.UpdateBatteries(0, Player.TotalBatteries);
    }

    private void OnPodReady(EscapePod pod)
    {
        _escapePod = pod;
    }

    private void HandleBatteryProgress(int current, int total)
    {
        _hud.UpdateBatteries(current, total);

        if (current >= total && _escapePod != null)
        {
            _escapePod.UnlockConsole();
        }
    }

    public override void _ExitTree()
    {
        if (_player != null)
        {
            _player.OnOxygenChanged -= _hud.UpdateOxygen;
            _player.OnBatteriesChanged -= HandleBatteryProgress;
        }

        if (_mazeGen != null)
        {
            _mazeGen.OnEscapePodSpawned -= OnPodReady;
        }
    }
}