using System.Drawing;

namespace MintPlayer.Bacon.Shapes;

public abstract class BaconLine : BaconShape
{
    public Pen Pen { get; set; } = new Pen(Color.Black, 1f);
}
