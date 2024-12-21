using System.Numerics;

namespace Bacon.Core.Shapes;

public class Point<TCoordinate> : Shape<TCoordinate>
    where TCoordinate : struct, INumber<TCoordinate>
{
    public Coordinate<TCoordinate> Coordinate { get; set; }
}
