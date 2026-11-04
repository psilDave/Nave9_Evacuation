#nullable enable

using System;
using Godot;

namespace Nave9_Evacuation.Scripts;

/// <summary>
/// First-person character controller.
/// Handles physics movement, mouse/touch camera control, oxygen depletion loop,
/// raycast-based item interaction, and triggers game-over upon suffocation.
/// </summary>
public partial class Player : CharacterBody3D
{
    [ExportGroup("Movement Settings")]
    [Export] public float Speed { get; set; } = 4.5f;
    [Export] public float MouseSensitivity { get; set; } = 0.0025f;
    [Export] public float TouchSensitivity { get; set; } = 0.004f;

    [ExportGroup("Survival Settings")]
    [Export] public float MaxOxygen { get; set; } = 100.0f;

    /// <summary>
    /// Current remaining oxygen in seconds.
    /// </summary>
    public float CurrentOxygen { get; private set; }

    /// <summary>
    /// Number of batteries collected so far.
    /// </summary>
    public int BatteriesCollected { get; private set; }

    /// <summary>
    /// Total batteries required to power the escape pod.
    /// </summary>
    public const int TotalBatteries = 4;

    private Node3D _head = null!;
    private Camera3D _camera = null!;
    private RayCast3D _interactRay = null!;
    private bool _isMobile;

    /// <summary>
    /// Dispatched when player oxygen level changes.
    /// Parameters: (currentOxygen, maxOxygen).
    /// </summary>
    public event Action<float, float>? OnOxygenChanged;

    /// <summary>
    /// Dispatched when player collects a battery.
    /// Parameters: (batteriesCollected, totalBatteries).
    /// </summary>
    public event Action<int, int>? OnBatteriesChanged;

    /// <summary>
    /// Dispatched when the player points at or away from an interactable object.
    /// Parameter: prompt text to show, or empty string to hide.
    /// </summary>
    public event Action<string>? OnPromptChanged;

    /// <summary>
    /// Initializes oxygen supply, retrieves node references, and captures mouse on desktop.
    /// </summary>
    public override void _Ready()
    {
        CurrentOxygen = MaxOxygen;
        _head = GetNode<Node3D>("Head");
        _camera = GetNode<Camera3D>("Head/Camera3D");
        _interactRay = GetNode<RayCast3D>("Head/RayCast3D");

        _isMobile = OS.HasFeature("mobile") || OS.GetName() == "Android";

        if (!_isMobile)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }

    /// <summary>
    /// Handles unhandled input events such as mouse motion on PC and touch screen drag on mobile.
    /// </summary>
    /// <param name="event">The input event to process.</param>
    public override void _UnhandledInput(InputEvent @event)
    {
        // Desktop mouse look (yaw and pitch)
        if (!_isMobile && @event is InputEventMouseMotion mouseMotion &&
            Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            RotateCamera(mouseMotion.Relative, MouseSensitivity);
        }

        // Mobile touch look (right half of the screen)
        if (@event is InputEventScreenDrag screenDrag)
        {
            float screenHalfWidth = GetViewport().GetVisibleRect().Size.X * 0.4f;
            if (screenDrag.Position.X > screenHalfWidth)
            {
                RotateCamera(screenDrag.Relative, TouchSensitivity);
            }
        }

        // Toggle mouse cursor visibility on PC when pressing Escape
        if (!_isMobile && Input.IsActionJustPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
                ? Input.MouseModeEnum.Visible
                : Input.MouseModeEnum.Captured;
        }
    }

    /// <summary>
    /// Rotates the player body horizontally (yaw) and camera head vertically (pitch) with clamping.
    /// </summary>
    /// <param name="relativeMove">Delta movement from mouse or touch drag.</param>
    /// <param name="sensitivity">Sensitivity scalar.</param>
    private void RotateCamera(Vector2 relativeMove, float sensitivity)
    {
        RotateY(-relativeMove.X * sensitivity);
        Vector3 headRot = _head.Rotation;
        headRot.X -= relativeMove.Y * sensitivity;
        headRot.X = Mathf.Clamp(headRot.X, Mathf.DegToRad(-80), Mathf.DegToRad(80));
        _head.Rotation = headRot;
    }

    /// <summary>
    /// Fixed-step physics tick.
    /// Updates oxygen countdown, computes movement direction and velocity,
    /// and tests raycast collisions for interactable items.
    /// </summary>
    /// <param name="delta">Elapsed physics time in seconds.</param>
    public override void _PhysicsProcess(double delta)
    {
        // Decrement oxygen reserve and notify subscribers
        CurrentOxygen -= (float)delta;
        OnOxygenChanged?.Invoke(CurrentOxygen, MaxOxygen);
        if (CurrentOxygen <= 0)
        {
            Die();
            return;
        }

        // Compute 3D direction vector from 2D input axes
        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).LimitLength(1.0f);

        Vector3 vel = Velocity;
        vel.X = direction.X * Speed;
        vel.Z = direction.Z * Speed;
        Velocity = vel;
        MoveAndSlide();

        // Check interaction raycast for collectible batteries
        if (_interactRay.IsColliding() && _interactRay.GetCollider() is Battery)
        {
            OnPromptChanged?.Invoke($"[{(_isMobile ? "PEGAR" : "E")}] Recolher Bateria");

            if (Input.IsActionJustPressed("interact"))
            {
                BatteriesCollected++;
                OnBatteriesChanged?.Invoke(BatteriesCollected, TotalBatteries);
                ((Battery)_interactRay.GetCollider()).Collect();
            }
        }
        else
        {
            OnPromptChanged?.Invoke("");
        }
    }

    /// <summary>
    /// Handles player death when oxygen is depleted.
    /// Restores mouse visibility and transitions to the GameOver scene.
    /// </summary>
    private void Die()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetTree().ChangeSceneToFile("res://Scenes/GameOver.tscn");
    }
}
