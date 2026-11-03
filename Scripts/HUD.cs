using Godot;

namespace Nave9_Evacuation.Scripts;

public partial class HUD : CanvasLayer
{
    private ProgressBar _oxygenBar = null!;
    private Label _oxygenText = null!;
    private Label _batteryText = null!;

    public override void _Ready()
    {
        _oxygenBar = GetNode<ProgressBar>("OxygenBar");
        _oxygenText = GetNode<Label>("OxygenText");
        _batteryText = GetNode<Label>("BatteryCountText");
    }

    public void UpdateOxygen(float current, float max)
    {
        _oxygenBar.MaxValue = max;
        _oxygenBar.Value = Mathf.Max(current, 0);
        _oxygenText.Text = $"O2 Reserva: {Mathf.CeilToInt(current)}s";
    }

    public void UpdateBatteries(int collected, int total)
    {
        _batteryText.Text = $"Baterias de Suporte: {collected} / {total}";
    }
}