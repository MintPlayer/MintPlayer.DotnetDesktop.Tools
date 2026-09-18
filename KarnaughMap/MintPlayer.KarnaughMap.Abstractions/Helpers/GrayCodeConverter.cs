namespace MintPlayer.KarnaughMap.Helpers;

/// <summary>
/// Converts between an ordinary index and its Gray code. Adjacent Gray codes differ in
/// exactly one bit, which is what makes neighbouring cells of a Karnaugh map differ in one
/// variable - the whole reason the map works.
/// </summary>
// Widened from internal to public: it moved into the abstractions assembly, and it is a
// small well-defined primitive worth exposing (and worth testing directly).
public static class GrayCodeConverter
{
    /// <summary>Converts an index to its Gray code.</summary>
    public static int Decimal2Gray(int n)
    {
        return n ^ (n >> 1);
    }

    /// <summary>Converts a Gray code back to an index.</summary>
    public static int Gray2Decimal(int n)
    {
        var mask = n >> 1;
        while (mask != 0)
        {
            n ^= mask;
            mask >>= 1;
        }
        return n;
    }
}
