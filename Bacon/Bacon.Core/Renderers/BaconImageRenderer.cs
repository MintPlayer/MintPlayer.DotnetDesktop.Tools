using Bacon.Core.Layers;
using System.Drawing;
using System.Numerics;

namespace Bacon.Core.Renderers;

internal class BaconImageRenderer
{
    public async Task<Bitmap> Render<TCoordinate>(BaconImage<TCoordinate> image, CancellationToken cancellationToken = default)
        where TCoordinate : struct, INumber<TCoordinate>
    {
        var bitmap = new Bitmap(image.Width, image.Height);
        var graphics = Graphics.FromImage(bitmap);
        await RenderInternal(image, graphics, cancellationToken);
        return bitmap;
    }

    public async Task Render<TCoordinate>(BaconImage<TCoordinate> image, Graphics graphics, CancellationToken cancellationToken = default)
        where TCoordinate : struct, INumber<TCoordinate>
    {
        await RenderInternal(image, graphics, cancellationToken);
    }

    private async Task RenderInternal<TCoordinate>(BaconImage<TCoordinate> image, Graphics graphics, CancellationToken cancellationToken = default)
        where TCoordinate : struct, INumber<TCoordinate>
    {
        foreach (var layer in image.Layers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (layer)
            {
                case PaintLayer<TCoordinate> paintLayer:
                    graphics.DrawImage(paintLayer.Bitmap, new Point(0, 0));
                    break;
                case ShapeLayer<TCoordinate> shapeLayer:
                    foreach (var shape in shapeLayer.Shapes)
                    {

                    }
                    break;
            }
        }
    }
}
