using System.Drawing;
using MintPlayer.Bacon.Shapes;
using MintPlayer.ObservableCollection; // adjust if needed

namespace MintPlayer.Bacon.Layers;

public class BaconShapeLayer : BaconLayer
{
    public MintPlayer.ObservableCollection.ObservableCollection<BaconShape> Shapes { get; } = new();
    public override void Draw(Graphics g)
    {
        if (!Visible) return;
        foreach (var shape in Shapes)
        {
            shape.Draw(g);
        }
    }
}
