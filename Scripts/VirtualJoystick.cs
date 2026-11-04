#nullable enable

using Godot;

namespace Nave9_Evacuation.Scripts;

/// <summary>
/// Custom on-screen virtual analog joystick control for mobile platforms.
/// Handles multi-touch inputs, rendering a dynamic analog stick with configurable deadzone,
/// and dispatching direction vectors as Godot input actions ("move_forward", "move_back", "move_left", "move_right").
/// </summary>
public partial class VirtualJoystick : Control
{
    /// <summary>
    /// Maximum displacement radius (in pixels) the joystick knob can move from center.
    /// </summary>
    [Export] public float MaxRadius { get; set; } = 75.0f;

    /// <summary>
    /// Normalized input deadzone threshold [0.0 - 1.0]. Inputs below this magnitude are treated as zero.
    /// </summary>
    [Export] public float Deadzone { get; set; } = 0.15f;

    private Vector2 _center;
    private Vector2 _knobOffset = Vector2.Zero;
    private int _touchIndex = -1;

    /// <summary>
    /// Configures minimum widget boundaries and calculates the center reference.
    /// </summary>
    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(MaxRadius * 2, MaxRadius * 2);
        _center = Size / 2.0f;
    }

    /// <summary>
    /// Custom 2D drawing routine rendering the base circle, border outline, and interactive thumb stick.
    /// </summary>
    public override void _Draw()
    {
        _center = Size / 2.0f;

        // Draw translucent dark circular background
        DrawCircle(_center, MaxRadius, new Color(0.12f, 0.12f, 0.12f, 0.6f));

        // Draw outer ring boundary
        DrawArc(_center, MaxRadius, 0, Mathf.Tau, 48, new Color(0.45f, 0.45f, 0.45f, 0.5f), 2.0f);

        // Draw joystick knob at current offset position
        DrawCircle(_center + _knobOffset, MaxRadius * 0.4f, new Color(0.85f, 0.85f, 0.85f, 0.8f));
    }

    /// <summary>
    /// Processes touch events, capturing the finger index on press and releasing on lift.
    /// </summary>
    /// <param name="event">The input event to process.</param>
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventScreenTouch touch)
        {
            // Initial touch detection within the joystick interaction area
            if (touch.Pressed && _touchIndex == -1)
            {
                Vector2 localPos = touch.Position - GlobalPosition;
                if (localPos.DistanceTo(_center) <= MaxRadius * 1.4f)
                {
                    _touchIndex = touch.Index;
                    UpdateJoystick(localPos);
                }
            }
            // Release detection for the active touch finger
            else if (!touch.Pressed && touch.Index == _touchIndex)
            {
                ResetJoystick();
            }
        }
        // Continuous drag updates while finger is moving
        else if (@event is InputEventScreenDrag drag && drag.Index == _touchIndex)
        {
            Vector2 localPos = drag.Position - GlobalPosition;
            UpdateJoystick(localPos);
        }
    }

    /// <summary>
    /// Computes clamped knob position, schedules a canvas redraw, and maps offset to movement actions.
    /// </summary>
    /// <param name="localPos">The current local touch position relative to the control.</param>
    private void UpdateJoystick(Vector2 localPos)
    {
        Vector2 delta = localPos - _center;
        _knobOffset = delta.LimitLength(MaxRadius);
        QueueRedraw();

        Vector2 output = _knobOffset / MaxRadius;
        if (output.Length() < Deadzone)
        {
            output = Vector2.Zero;
        }

        ApplyInputActions(output);
    }

    /// <summary>
    /// Resets the joystick stick to center and releases all associated input actions.
    /// </summary>
    private void ResetJoystick()
    {
        _touchIndex = -1;
        _knobOffset = Vector2.Zero;
        QueueRedraw();
        ApplyInputActions(Vector2.Zero);
    }

    /// <summary>
    /// Maps a 2D normalized directional vector into Godot action presses and releases.
    /// </summary>
    /// <param name="vector">Normalized input vector (-1.0 to 1.0 on X and Y).</param>
    private void ApplyInputActions(Vector2 vector)
    {
        // Horizontal axis mapping (left / right)
        if (vector.X > 0)
        {
            Input.ActionPress("move_right", vector.X);
            Input.ActionRelease("move_left");
        }
        else if (vector.X < 0)
        {
            Input.ActionPress("move_left", -vector.X);
            Input.ActionRelease("move_right");
        }
        else
        {
            Input.ActionRelease("move_left");
            Input.ActionRelease("move_right");
        }

        // Vertical axis mapping (forward / backward)
        if (vector.Y > 0)
        {
            Input.ActionPress("move_back", vector.Y);
            Input.ActionRelease("move_forward");
        }
        else if (vector.Y < 0)
        {
            Input.ActionPress("move_forward", -vector.Y);
            Input.ActionRelease("move_back");
        }
        else
        {
            Input.ActionRelease("move_forward");
            Input.ActionRelease("move_back");
        }
    }
}
