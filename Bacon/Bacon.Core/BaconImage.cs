using System.Drawing;
using MintPlayer.Bacon.Layers;

namespace MintPlayer.Bacon;

public class BaconImage
{
    public int Width { get; set; }
    public int Height { get; set; }

    public MintPlayer.ObservableCollection.ObservableCollection<BaconLayer> Layers { get; } = new();

    public Bitmap Render()
    {
        var bmp = new Bitmap(Width, Height);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Transparent);
        foreach (var layer in Layers.Where(l => l.Visible)) layer.Draw(g);
        // Control points already drawn inside shapes when Selected
        return bmp;
    }
}
