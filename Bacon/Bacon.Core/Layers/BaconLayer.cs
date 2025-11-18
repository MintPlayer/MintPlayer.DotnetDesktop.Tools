using System.Drawing;
using MintPlayer.Bacon.Shapes;

namespace MintPlayer.Bacon.Layers;

public abstract class BaconLayer
{
    public bool Visible { get; set; } = true;
    public string Name { get; set; } = string.Empty;
    public abstract void Draw(Graphics g);
}
