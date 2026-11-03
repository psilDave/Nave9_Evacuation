using Godot;

namespace Nave9_Evacuation.Scripts;

public partial class Battery : StaticBody3D
{
    [Export] public string ItemName { get; set; } = "Bateria de Lítio-Titânio";

    public void Collect()
    {
        QueueFree();
    }
}
