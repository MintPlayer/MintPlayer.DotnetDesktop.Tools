using System.Drawing;
using System.Numerics;

namespace Bacon.Core;

public class BaconImage<TCoordinate>
    where TCoordinate : struct, INumber<TCoordinate>
{
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required List<Layers.Layer<TCoordinate>> Layers { get; set; } = [];
}
