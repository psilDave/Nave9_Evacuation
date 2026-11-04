#nullable enable

using Godot;

namespace Nave9_Evacuation.Scripts;

/// <summary>
/// Main gameplay controller and orchestrator for Nave 9: Evacuation.
/// Responsible for bootstrapping the game scene, wiring inter-system events
/// (Player, HUD, MazeGenerator), and handling game progression milestones.
/// </summary>
public partial class Main : Node3D
{
    // ==========================================
    // Scene References
    // ==========================================
    [ExportGroup("Core References")]
    [Export] private Player _player = null!;
    [Export] private HUD _hud = null!;
    [Export] private MazeGenerator _mazeGen = null!;
    [Export] private Node3D _itemsContainer = null!;

    [ExportGroup("Audio")]
    [Export] private AudioStreamPlayer _collectSound = null!;
    [Export] private AudioStreamPlayer _unlockSound = null!;

    /// <summary>
    /// Reference to the escape pod instance spawned dynamically by the maze generator.
    /// </summary>
    private EscapePod? _escapePod;

    /// <summary>
    /// Godot lifecycle hook called when the node and its children enter the scene tree.
    /// Initializes references, subscribes to gameplay events, and starts maze generation.
    /// </summary>
    public override void _Ready()
    {
        // Resolve scene node references (uses inspector exports if assigned, with GetNode fallbacks)
        _player ??= GetNode<Player>("Player");
        _hud ??= GetNode<HUD>("HUD");
        _mazeGen ??= GetNode<MazeGenerator>("MazeGenerator");
        _itemsContainer ??= GetNode<Node3D>("Items");

        _collectSound ??= GetNode<AudioStreamPlayer>("CollectSound");
        _unlockSound ??= GetNode<AudioStreamPlayer>("UnlockSound");

        // Listen for when the procedural generator spawns the escape pod
        _mazeGen.OnEscapePodSpawned += OnPodReady;

        // Wire player status and interaction events to the HUD and game progression
        _player.OnOxygenChanged += _hud.UpdateOxygen;
        _player.OnBatteriesChanged += HandleBatteryProgress;
        _player.OnPromptChanged += _hud.SetInteractPrompt;

        // Generate the procedural maze, placing walls, the player, and collectible items
        _mazeGen.GenerateAndBuild(_player, _itemsContainer);

        // Perform initial HUD setup with default starting values
        _hud.UpdateOxygen(_player.MaxOxygen, _player.MaxOxygen);
        _hud.UpdateBatteries(0, Player.TotalBatteries);
    }

    /// <summary>
    /// Callback triggered when the MazeGenerator instantiates the EscapePod.
    /// Stores the reference so it can be unlocked once all batteries are acquired.
    /// </summary>
    /// <param name="pod">The spawned EscapePod instance.</param>
    private void OnPodReady(EscapePod pod)
    {
        _escapePod = pod;
    }

    /// <summary>
    /// Handles battery collection progress updates.
    /// Updates the HUD, plays feedback audio, and unlocks the escape pod upon completing the objective.
    /// </summary>
    /// <param name="current">Total number of batteries collected so far.</param>
    /// <param name="total">Total number of batteries required to evacuate.</param>
    private void HandleBatteryProgress(int current, int total)
    {
        // Update user interface counter
        _hud.UpdateBatteries(current, total);

        // Play collection audio feedback
        _collectSound.Play();

        // Check victory objective: all batteries gathered
        if (current >= total && _escapePod != null)
        {
            _escapePod.UnlockConsole();
            _unlockSound.Play();
        }
    }

    /// <summary>
    /// Godot lifecycle hook called when this node is removed from the scene tree.
    /// Safely unsubscribes from C# delegates to prevent memory leaks and dangling references.
    /// </summary>
    public override void _ExitTree()
    {
        // Unsubscribe from player events if player instance is still valid in memory
        if (GodotObject.IsInstanceValid(_player))
        {
            _player.OnOxygenChanged -= _hud.UpdateOxygen;
            _player.OnBatteriesChanged -= HandleBatteryProgress;
            _player.OnPromptChanged -= _hud.SetInteractPrompt;
        }

        // Unsubscribe from maze generator events
        if (GodotObject.IsInstanceValid(_mazeGen))
        {
            _mazeGen.OnEscapePodSpawned -= OnPodReady;
        }
    }
}
