using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Serialization;

namespace MintPlayer.Bacon.Images.Data;

/// <summary>
/// A JSON-serializable representation of a pen.
/// </summary>
public class SerializablePen
{
    public SerializableColor Color { get; set; } = new(System.Drawing.Color.Black);
    public float Width { get; set; } = 1f;
    public DashStyle DashStyle { get; set; } = DashStyle.Solid;
    public DashCap DashCap { get; set; } = DashCap.Flat;
    public LineCap StartCap { get; set; } = LineCap.Flat;
    public LineCap EndCap { get; set; } = LineCap.Flat;

    [JsonIgnore]
    public Pen Pen
    {
        get
        {
            var pen = new Pen(Color, Width)
            {
                DashStyle = DashStyle,
                DashCap = DashCap,
                StartCap = StartCap,
                EndCap = EndCap
            };
            return pen;
        }
    }

    public SerializablePen() { }

    public SerializablePen(Pen pen)
    {
        Color = pen.Color;
        Width = pen.Width;
        DashStyle = pen.DashStyle;
        DashCap = pen.DashCap;
        StartCap = pen.StartCap;
        EndCap = pen.EndCap;
    }

    public SerializablePen(Color color, float width = 1f)
    {
        Color = color;
        Width = width;
    }
}
