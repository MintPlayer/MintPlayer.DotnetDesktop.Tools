using System.Numerics;

namespace Bacon.Core.Shapes;

public abstract class Line<TCoordinate> : Shape<TCoordinate>
    where TCoordinate : struct, INumber<TCoordinate>
{

}
