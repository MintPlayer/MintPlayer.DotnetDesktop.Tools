using System.Drawing;
using System.Numerics;

namespace Bacon.Core.Layers;

public abstract class Layer<TCoordinate>
    where TCoordinate : struct, INumber<TCoordinate>
{
}
