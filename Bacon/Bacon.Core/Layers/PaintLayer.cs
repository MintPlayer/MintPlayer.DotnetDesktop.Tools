using System.Drawing;
using System.Numerics;

namespace Bacon.Core.Layers;

public class PaintLayer<TCoordinate> : Layer<TCoordinate>
    where TCoordinate : struct, INumber<TCoordinate>
{
    public Bitmap Bitmap { get; set; }
}
