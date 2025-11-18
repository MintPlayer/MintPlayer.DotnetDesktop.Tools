using System.Drawing;

namespace MintPlayer.Bacon.Shapes;

public abstract class BaconPlane : BaconShape
{
    public Pen Pen { get; set; } = new Pen(Color.Black, 1f);
    public Brush Brush { get; set; } = new SolidBrush(Color.Transparent);
}
