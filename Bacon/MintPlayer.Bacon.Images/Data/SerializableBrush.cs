using System.Drawing;
using System.Text.Json.Serialization;

namespace MintPlayer.Bacon.Images.Data;

/// <summary>
/// A JSON-serializable representation of a brush.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(SerializableSolidBrush), "solid")]
[JsonDerivedType(typeof(SerializableLinearGradientBrush), "linearGradient")]
public abstract class SerializableBrush
{
    [JsonIgnore]
    public abstract Brush Brush { get; }
}

public class SerializableSolidBrush : SerializableBrush
{
    public SerializableColor Color { get; set; } = new(System.Drawing.Color.White);

    [JsonIgnore]
    public override Brush Brush => new SolidBrush(Color);

    public SerializableSolidBrush() { }

    public SerializableSolidBrush(Color color)
    {
        Color = color;
    }
}

public class SerializableLinearGradientBrush : SerializableBrush
{
    public SerializablePoint Point1 { get; set; } = new();
    public SerializablePoint Point2 { get; set; } = new() { X = 100, Y = 100 };
    public SerializableColor Color1 { get; set; } = new(System.Drawing.Color.White);
    public SerializableColor Color2 { get; set; } = new(System.Drawing.Color.Black);

    [JsonIgnore]
    public override Brush Brush =>
        new System.Drawing.Drawing2D.LinearGradientBrush(Point1, Point2, Color1, Color2);

    public SerializableLinearGradientBrush() { }

    public SerializableLinearGradientBrush(PointF point1, PointF point2, Color color1, Color color2)
    {
        Point1 = point1;
        Point2 = point2;
        Color1 = color1;
        Color2 = color2;
    }
}
