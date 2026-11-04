#nullable enable

using Godot;

namespace Nave9_Evacuation.Scripts;

/// <summary>
/// Heads-Up Display (HUD) controller.
/// Manages player telemetry displays (oxygen progress bar, battery count),
/// dynamic warning colors, interaction prompt messages, and mobile-specific touch controls.
/// </summary>
public partial class HUD : CanvasLayer
{
    private ProgressBar _oxygenBar = null!;
    private Label _oxygenText = null!;
    private Label _batteryText = null!;
    private Label? _interactPrompt;

    /// <summary>
    /// Initializes UI component references, detects mobile operating systems,
    /// and configures mobile-specific control bindings if running on mobile.
    /// </summary>
    public override void _Ready()
    {
        _oxygenBar = GetNode<ProgressBar>("OxygenBar");
        _oxygenText = GetNode<Label>("OxygenText");
        _batteryText = GetNode<Label>("BatteryCountText");
        _interactPrompt = GetNodeOrNull<Label>("InteractPrompt");

        // Automatically toggle mobile overlay based on device features
        var mobileControls = GetNode<Control>("MobileControls");
        mobileControls.Visible = OS.HasFeature("mobile") || OS.GetName() == "Android";

        BindMobileButton("MobileControls/BtnInteract", "interact");
    }

    /// <summary>
    /// Helper method to bind on-screen touch buttons to Godot input actions.
    /// </summary>
    /// <param name="nodePath">Path to the Button node.</param>
    /// <param name="actionName">The Godot input action to trigger.</param>
    private void BindMobileButton(string nodePath, string actionName)
    {
        var btn = GetNodeOrNull<Button>(nodePath);
        if (btn == null) return;

        btn.FocusMode = Control.FocusModeEnum.None;
        btn.ButtonDown += () => Input.ActionPress(actionName);
        btn.ButtonUp += () => Input.ActionRelease(actionName);
    }

    /// <summary>
    /// Updates the oxygen bar progress and digital timer readout.
    /// Shifts text color to critical red when oxygen is below 30 seconds.
    /// </summary>
    /// <param name="current">Current remaining oxygen in seconds.</param>
    /// <param name="max">Maximum oxygen capacity in seconds.</param>
    public void UpdateOxygen(float current, float max)
    {
        _oxygenBar.MaxValue = max;
        _oxygenBar.Value = Mathf.Max(current, 0);
        _oxygenText.Text = $"O2 Reserva: {Mathf.CeilToInt(current)}s";

        // Critical threshold: change color to red warning below 30s
        _oxygenText.Modulate = current <= 30.0f
            ? new Color("#FF3D00")
            : new Color("#00E5FF");
    }

    /// <summary>
    /// Updates the on-screen battery collection counter.
    /// </summary>
    /// <param name="collected">Number of batteries picked up.</param>
    /// <param name="total">Total batteries required to evacuate.</param>
    public void UpdateBatteries(int collected, int total)
    {
        _batteryText.Text = $"Baterias de Suporte: {collected} / {total}";
    }

    /// <summary>
    /// Displays or clears contextual interaction prompt text in the center of the screen.
    /// </summary>
    /// <param name="message">Prompt message text (e.g. "[E] Recolher Bateria") or empty string to hide.</param>
    public void SetInteractPrompt(string message)
    {
        if (_interactPrompt != null)
        {
            _interactPrompt.Text = message;
        }
    }
}
