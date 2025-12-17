using System.Drawing;
using System.Text.Json.Serialization;
using MintPlayer.Bacon.Images.Data;

namespace MintPlayer.Bacon.Images.Shapes;

/// <summary>
/// Abstract base class for all shapes.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(LineSegment), "lineSegment")]
[JsonDerivedType(typeof(Curve), "curve")]
[JsonDerivedType(typeof(CircleSegment), "circleSegment")]
[JsonDerivedType(typeof(Polygon), "polygon")]
[JsonDerivedType(typeof(Circle), "circle")]
[JsonDerivedType(typeof(ClosedCurve), "closedCurve")]
[JsonDerivedType(typeof(Rectangle), "rectangle")]
[JsonDerivedType(typeof(Ellipse), "ellipse")]
[JsonDerivedType(typeof(Arc), "arc")]
[JsonDerivedType(typeof(Bezier), "bezier")]
public abstract class Shape
{
    /// <summary>
    /// Name of the shape for identification.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Whether the shape is visible.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Whether the shape is locked (cannot be edited).
    /// </summary>
    public bool IsLocked { get; set; } = false;

    /// <summary>
    /// Get all control points that can be dragged by the user.
    /// </summary>
    [JsonIgnore]
    public abstract IReadOnlyList<SerializablePoint> ControlPoints { get; }

    /// <summary>
    /// Update a control point at the specified index.
    /// </summary>
    public abstract void SetControlPoint(int index, PointF newPosition);

    /// <summary>
    /// Draw the shape on the graphics surface.
    /// </summary>
    public abstract void Draw(Graphics g);

    /// <summary>
    /// Get the bounding rectangle of the shape.
    /// </summary>
    [JsonIgnore]
    public abstract RectangleF Bounds { get; }

    /// <summary>
    /// Check if a point is near any control point (within tolerance).
    /// Returns the index of the control point, or -1 if none.
    /// </summary>
    public int HitTestControlPoint(PointF point, float tolerance = 5f)
    {
        for (int i = 0; i < ControlPoints.Count; i++)
        {
            var cp = ControlPoints[i];
            if (Math.Abs(cp.X - point.X) <= tolerance && Math.Abs(cp.Y - point.Y) <= tolerance)
            {
                return i;
            }
        }
        return -1;
    }
}
