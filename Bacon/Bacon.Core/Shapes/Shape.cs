using System.Numerics;

namespace Bacon.Core.Shapes;

public abstract class Shape<TCoordinate>
    where TCoordinate : struct, INumber<TCoordinate>
{
}
