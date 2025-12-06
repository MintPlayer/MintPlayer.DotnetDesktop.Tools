using System.Drawing;
using System.Drawing.Drawing2D;
using MintPlayer.Bacon.Images.Shapes;

namespace MintPlayer.Bacon.Images.Layers;

/// <summary>
/// A layer that contains vector shapes.
/// </summary>
public class ShapeLayer : Layer
{
    /// <summary>
    /// The shapes in this layer.
    /// </summary>
    public List<Shape> Shapes { get; set; } = [];

    public override void Draw(Graphics g, int width, int height)
    {
        if (!IsVisible) return;

        if (Opacity < 1.0f)
        {
            // Draw to a temporary bitmap for opacity support
            using var tempBitmap = new Bitmap(width, height);
            using var tempGraphics = Graphics.FromImage(tempBitmap);
            tempGraphics.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (var shape in Shapes)
            {
                shape.Draw(tempGraphics);
            }

            // Create a color matrix for opacity
            var colorMatrix = new System.Drawing.Imaging.ColorMatrix
            {
                Matrix33 = Opacity
            };

            using var imageAttributes = new System.Drawing.Imaging.ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix);

            g.DrawImage(tempBitmap,
                new System.Drawing.Rectangle(0, 0, width, height),
                0, 0, width, height,
                GraphicsUnit.Pixel,
                imageAttributes);
        }
        else
        {
            foreach (var shape in Shapes)
            {
                shape.Draw(g);
            }
        }
    }
}
