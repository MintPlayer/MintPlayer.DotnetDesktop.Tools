using System.Globalization;

namespace MintPlayer.ThreeDee.Tools;

/// <summary>VCB number parsing (honors the machine locale, e.g. comma decimals).</summary>
static class Num
{
    public static bool TryParse(string s, out float v) =>
        float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out v)
        || float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out v);

    public static string Fmt(float v) => v.ToString("0.###", CultureInfo.CurrentCulture);
}
