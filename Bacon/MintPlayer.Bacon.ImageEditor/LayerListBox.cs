using System.Drawing;
using System.Drawing.Drawing2D;
using MintPlayer.Bacon.Images.Layers;

namespace MintPlayer.Bacon.ImageEditor;

/// <summary>
/// A custom ListBox that displays layer items with icons.
/// </summary>
public class LayerListBox : ListBox
{
    private const int IconSize = 24;
    private const int ItemPadding = 4;

    private static readonly Color ShapeLayerColor = Color.FromArgb(100, 150, 200);
    private static readonly Color PaintLayerColor = Color.FromArgb(150, 200, 100);

    public LayerListBox()
    {
        DrawMode = DrawMode.OwnerDrawFixed;
        ItemHeight = IconSize + ItemPadding * 2;
        IntegralHeight = false;
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= Items.Count) return;

        e.DrawBackground();

        var item = Items[e.Index];
        if (item is not Layer layer) return;

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Calculate icon area
        var iconRect = new Rectangle(
            e.Bounds.X + ItemPadding,
            e.Bounds.Y + ItemPadding,
            IconSize,
            IconSize);

        // Draw layer type icon
        DrawLayerIcon(g, iconRect, layer);

        // Draw visibility indicator
        if (!layer.IsVisible)
        {
            using var dimBrush = new SolidBrush(Color.FromArgb(180, e.BackColor));
            g.FillRectangle(dimBrush, e.Bounds);

            // Draw "hidden" indicator
            using var hiddenPen = new Pen(Color.Red, 2);
            g.DrawLine(hiddenPen, iconRect.Left, iconRect.Top, iconRect.Right, iconRect.Bottom);
            g.DrawLine(hiddenPen, iconRect.Right, iconRect.Top, iconRect.Left, iconRect.Bottom);
        }

        // Draw lock indicator
        if (layer.IsLocked)
        {
            var lockRect = new Rectangle(iconRect.Right - 8, iconRect.Bottom - 8, 8, 8);
            using var lockBrush = new SolidBrush(Color.Orange);
            g.FillRectangle(lockBrush, lockRect);
            using var lockPen = new Pen(Color.DarkOrange, 1);
            g.DrawRectangle(lockPen, lockRect);
        }

        // Draw text (layer name and type)
        var textX = e.Bounds.X + ItemPadding * 2 + IconSize;
        var textY = e.Bounds.Y + ItemPadding;
        var textRect = new Rectangle(textX, textY, e.Bounds.Width - textX - ItemPadding, e.Bounds.Height - ItemPadding * 2);

        var layerType = layer switch
        {
            ShapeLayer sl => $"Shape ({sl.Shapes.Count})",
            PaintLayer => "Paint",
            _ => "Layer"
        };

        var nameText = string.IsNullOrEmpty(layer.Name) ? layerType : $"{layer.Name}\n{layerType}";

        var textColor = (e.State & DrawItemState.Selected) != 0 ? SystemColors.HighlightText : SystemColors.ControlText;
        if (!layer.IsVisible)
        {
            textColor = Color.FromArgb(128, textColor);
        }

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

    private void DrawLayerIcon(Graphics g, Rectangle bounds, Layer layer)
    {
        Color iconColor;

        if (layer is ShapeLayer)
        {
            iconColor = ShapeLayerColor;
            // Draw shape layer icon (overlapping shapes)
            using var brush = new SolidBrush(iconColor);
            using var pen = new Pen(Color.FromArgb(80, 120, 160), 1);

            var rect1 = new Rectangle(bounds.X + 2, bounds.Y + 2, 12, 12);
            var rect2 = new Rectangle(bounds.X + 8, bounds.Y + 8, 12, 12);

            g.FillRectangle(brush, rect1);
            g.DrawRectangle(pen, rect1);
            g.FillRectangle(brush, rect2);
            g.DrawRectangle(pen, rect2);
        }
        else if (layer is PaintLayer)
        {
            iconColor = PaintLayerColor;
            // Draw paint layer icon (brush stroke)
            using var brush = new SolidBrush(iconColor);
            using var pen = new Pen(Color.FromArgb(120, 160, 80), 2);

            g.FillEllipse(brush, bounds.X + 4, bounds.Y + 4, bounds.Width - 8, bounds.Height - 8);

            // Draw a wavy line to represent painting
            var points = new PointF[]
            {
                new(bounds.X + 4, bounds.Y + bounds.Height / 2),
                new(bounds.X + bounds.Width / 3, bounds.Y + 6),
                new(bounds.X + bounds.Width * 2 / 3, bounds.Y + bounds.Height - 6),
                new(bounds.X + bounds.Width - 4, bounds.Y + bounds.Height / 2)
            };
            g.DrawCurve(pen, points, 0.5f);
        }
        else
        {
            // Generic layer icon
            using var brush = new SolidBrush(Color.Gray);
            g.FillRectangle(brush, bounds.X + 4, bounds.Y + 4, bounds.Width - 8, bounds.Height - 8);
        }

        // Draw border
        using var borderPen = new Pen(Color.Gray, 1);
        g.DrawRectangle(borderPen, bounds);
    }
}
