using System.Drawing;
using System.Text.Json.Serialization;

namespace MintPlayer.Bacon.Images.Data;

/// <summary>
/// A JSON-serializable representation of a point.
/// </summary>
public class SerializablePoint
{
    public float X { get; set; }
    public float Y { get; set; }

    [JsonIgnore]
    public PointF Point
    {
        get => new(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    public SerializablePoint() { }

    public SerializablePoint(float x, float y)
    {
        X = x;
        Y = y;
    }

    public SerializablePoint(PointF point)
    {
        Point = point;
    }

    public static implicit operator PointF(SerializablePoint sp) => sp.Point;
    public static implicit operator SerializablePoint(PointF p) => new(p);
}
