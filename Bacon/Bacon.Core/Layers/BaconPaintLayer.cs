using System.Drawing;

namespace MintPlayer.Bacon.Layers;

public class BaconPaintLayer : BaconLayer
{
    public Bitmap? Bitmap { get; set; }
    public override void Draw(Graphics g)
    {
        if (!Visible) return;
        if (Bitmap != null)
        {
            g.DrawImage(Bitmap, 0, 0);
        }
    }
}
