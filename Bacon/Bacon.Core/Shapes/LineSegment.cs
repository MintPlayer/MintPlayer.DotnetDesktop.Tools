using System.Numerics;

namespace Bacon.Core.Shapes;

public class LineSegment<TCoordinate> : Line<TCoordinate>
    where TCoordinate : struct, INumber<TCoordinate>
{
    public Coordinate<TCoordinate> Start { get; set; }
    public Coordinate<TCoordinate> End { get; set; }
}
