using System.Numerics;

namespace Bacon.Core;

public struct Dimensions<TCoordinate>
    where TCoordinate : struct, INumber<TCoordinate>
{
    public TCoordinate Width { get; set; }
    public TCoordinate Height { get; set; }
}
