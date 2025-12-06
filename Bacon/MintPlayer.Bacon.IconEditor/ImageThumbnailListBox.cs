using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using MintPlayer.Bacon.Images;

namespace MintPlayer.Bacon.IconEditor;

/// <summary>
/// A custom ListBox that displays image thumbnails.
/// </summary>
public class ImageThumbnailListBox : ListBox
{
    private const int ThumbnailSize = 48;
    private const int ItemPadding = 4;
    private const int CheckerboardSize = 4;

    private static readonly Color CheckerColor1 = Color.FromArgb(204, 204, 204);
    private static readonly Color CheckerColor2 = Color.FromArgb(255, 255, 255);

    public ImageThumbnailListBox()
    {
        DrawMode = DrawMode.OwnerDrawFixed;
        ItemHeight = ThumbnailSize + ItemPadding * 2;
        IntegralHeight = false;
        DoubleBuffered = true;
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= Items.Count) return;

        e.DrawBackground();

        var item = Items[e.Index];
        if (item is not BaconImage image) return;

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Calculate thumbnail area
        var thumbnailRect = new Rectangle(
            e.Bounds.X + ItemPadding,
            e.Bounds.Y + ItemPadding,
            ThumbnailSize,
            ThumbnailSize);

        // Draw checkerboard background for transparency
        DrawCheckerboard(g, thumbnailRect);

        // Render and draw the image thumbnail
        using var renderedImage = image.Render();

        // Calculate scaling to fit in thumbnail
        var scale = Math.Min(
            (float)ThumbnailSize / renderedImage.Width,
            (float)ThumbnailSize / renderedImage.Height);

        var scaledWidth = (int)(renderedImage.Width * scale);
        var scaledHeight = (int)(renderedImage.Height * scale);
        var offsetX = thumbnailRect.X + (ThumbnailSize - scaledWidth) / 2;
        var offsetY = thumbnailRect.Y + (ThumbnailSize - scaledHeight) / 2;

        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.DrawImage(renderedImage, offsetX, offsetY, scaledWidth, scaledHeight);

        // Draw border around thumbnail
        using var borderPen = new Pen(Color.Gray, 1);
        g.DrawRectangle(borderPen, thumbnailRect);

        // Draw text (size)
        var textX = e.Bounds.X + ItemPadding * 2 + ThumbnailSize;
        var textY = e.Bounds.Y + ItemPadding;
        var textRect = new Rectangle(textX, textY, e.Bounds.Width - textX - ItemPadding, e.Bounds.Height - ItemPadding * 2);

        var sizeText = $"{image.Width} x {image.Height}";
        var nameText = string.IsNullOrEmpty(image.Name) ? sizeText : $"{image.Name}\n{sizeText}";

        var textColor = (e.State & DrawItemState.Selected) != 0 ? SystemColors.HighlightText : SystemColors.ControlText;
        using var brush = new SolidBrush(textColor);
        using var font = new Font(Font.FontFamily, 9, FontStyle.Regular);

        var format = new StringFormat
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Center
        };

        g.DrawString(nameText, font, brush, textRect, format);

        e.DrawFocusRectangle();
    }

    private void DrawCheckerboard(Graphics g, Rectangle bounds)
    {
        using var brush1 = new SolidBrush(CheckerColor1);
        using var brush2 = new SolidBrush(CheckerColor2);

        for (int y = bounds.Y; y < bounds.Bottom; y += CheckerboardSize)
        {
            for (int x = bounds.X; x < bounds.Right; x += CheckerboardSize)
            {
                var isEven = ((x - bounds.X) / CheckerboardSize + (y - bounds.Y) / CheckerboardSize) % 2 == 0;
                var brush = isEven ? brush1 : brush2;
                var rect = new Rectangle(x, y,
                    Math.Min(CheckerboardSize, bounds.Right - x),
                    Math.Min(CheckerboardSize, bounds.Bottom - y));
                g.FillRectangle(brush, rect);
            }
        }
    }
}
