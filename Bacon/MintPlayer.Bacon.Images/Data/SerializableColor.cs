using System.Drawing;
using System.Text.Json.Serialization;

namespace MintPlayer.Bacon.Images.Data;

/// <summary>
/// A JSON-serializable representation of a color.
/// </summary>
public class SerializableColor
{
    public byte A { get; set; } = 255;
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }

    [JsonIgnore]
    public Color Color
    {
        get => Color.FromArgb(A, R, G, B);
        set
        {
            A = value.A;
            R = value.R;
            G = value.G;
            B = value.B;
        }
    }

    public SerializableColor() { }

    public SerializableColor(Color color)
    {
        Color = color;
    }

    public static implicit operator Color(SerializableColor sc) => sc.Color;
    public static implicit operator SerializableColor(Color c) => new(c);
}
