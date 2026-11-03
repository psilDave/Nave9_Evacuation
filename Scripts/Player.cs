using System;
using Godot;

namespace Nave9_Evacuation.Scripts;

public partial class Player : CharacterBody3D
{
    [Export] public float Speed { get; set; } = 4.5f;
    [Export] public float MouseSensitivity { get; set; } = 0.0025f;
    [Export] public float MaxOxygen { get; set; } = 100.0f;

    public float CurrentOxygen { get; private set; }
    public int BatteriesCollected { get; private set; }

    public const int TotalBatteries = 4;

    private Node3D _head = null!;
    private Camera3D _camera = null!;
    private RayCast3D _interactRay = null!;

    public event Action<float, float>? OnOxygenChanged;
    public event Action<int, int>? OnBatteriesChanged;

    public override void _Ready()
    {
        CurrentOxygen = MaxOxygen;
        _head = GetNode<Node3D>("Head");
        _camera = GetNode<Camera3D>("Head/Camera3D");
        _interactRay = GetNode<RayCast3D>("Head/RayCast3D");

        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);
            Vector3 headRot = _head.Rotation;
            headRot.X -= mouseMotion.Relative.Y * MouseSensitivity;
            headRot.X = Mathf.Clamp(headRot.X, Mathf.DegToRad(-80), Mathf.DegToRad(80));
            _head.Rotation = headRot;
        }

        if (Input.IsActionJustPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
                ? Input.MouseModeEnum.Visible
                : Input.MouseModeEnum.Captured;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        CurrentOxygen -= (float)delta;
        OnOxygenChanged?.Invoke(CurrentOxygen, MaxOxygen);
        if (CurrentOxygen <= 0)
        {
            Die();
            return;
        }

        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        Vector3 vel = Velocity;
        vel.X = direction.X * Speed;
        vel.Z = direction.Z * Speed;
        Velocity = vel;
        MoveAndSlide();

        if (Input.IsActionJustPressed("interact") && _interactRay.IsColliding())
        {
            var collier = _interactRay.GetCollider();
            if (collier is Battery battery)
            {
                BatteriesCollected++;
                OnBatteriesChanged?.Invoke(BatteriesCollected, TotalBatteries);
                battery.Collect();
            }
        }
    }

    private void Die()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetTree().ChangeSceneToFile("res://Scenes/GameOver.tscn");
    }
}