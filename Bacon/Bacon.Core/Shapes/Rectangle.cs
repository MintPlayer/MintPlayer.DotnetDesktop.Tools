using System.Numerics;

namespace Bacon.Core.Shapes;

public class Rectangle<TCoordinate> : Plane<TCoordinate>
    where TCoordinate : struct, INumber<TCoordinate>
{
    public Coordinate<TCoordinate> Location { get; set; }
    public Dimensions<TCoordinate> Size { get; set; }
}
