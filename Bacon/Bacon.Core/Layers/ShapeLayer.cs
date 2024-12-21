using System.Drawing;
using System.Numerics;

namespace Bacon.Core.Layers;

public class ShapeLayer<TCoordinate> : Layer<TCoordinate>
    where TCoordinate : struct, INumber<TCoordinate>
{
    public List<Shapes.Shape<TCoordinate>> Shapes { get; set; } = [];
}
