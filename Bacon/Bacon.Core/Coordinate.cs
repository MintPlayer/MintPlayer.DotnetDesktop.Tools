using System.Numerics;

namespace Bacon.Core;

public struct Coordinate<TCoordinate>
    where TCoordinate : struct, INumber<TCoordinate>
{
    public TCoordinate X { get; set; }
    public TCoordinate Y { get; set; }
}
