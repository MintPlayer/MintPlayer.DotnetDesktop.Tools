using System.Numerics;

namespace Bacon.Core;

public class BaconImageSet<TCoordinate>
    where TCoordinate : struct, INumber<TCoordinate>
{
    public List<BaconImage<TCoordinate>> Images { get; set; } = [];
}
