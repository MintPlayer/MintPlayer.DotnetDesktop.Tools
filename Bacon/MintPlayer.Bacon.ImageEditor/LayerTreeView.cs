using System.Drawing;
using System.Drawing.Drawing2D;
using MintPlayer.Bacon.Images.Layers;
using MintPlayer.Bacon.Images.Shapes;
using Rectangle = System.Drawing.Rectangle;

namespace MintPlayer.Bacon.ImageEditor;

/// <summary>
/// A TreeView that displays layers with their shapes nested underneath.
/// </summary>
public class LayerTreeView : TreeView
{
    private static readonly Color ShapeLayerColor = Color.FromArgb(100, 150, 200);
    private static readonly Color PaintLayerColor = Color.FromArgb(150, 200, 100);
    private static readonly Color ShapeColor = Color.FromArgb(180, 180, 220);

    public LayerTreeView()
    {
        DoubleBuffered = true;
        DrawMode = TreeViewDrawMode.OwnerDrawAll;
        ShowLines = true;
        ShowPlusMinus = true;
        ShowRootLines = true;
        FullRowSelect = true;
        HideSelection = false;
        ItemHeight = 24;
    }

    /// <summary>
    /// Populate the tree with layers and their shapes.
    /// </summary>
    public void PopulateLayers(IEnumerable<Layer> layers)
    {
        BeginUpdate();
        try
        {
            var selectedTag = SelectedNode?.Tag;
            Nodes.Clear();

            // Add layers in reverse order (top layer first)
            var layerList = layers.ToList();
            for (int i = layerList.Count - 1; i >= 0; i--)
            {
                var layer = layerList[i];
                var layerNode = CreateLayerNode(layer);
                Nodes.Add(layerNode);

                // Add shapes if it's a shape layer
                if (layer is ShapeLayer shapeLayer)
                {
                    foreach (var shape in shapeLayer.Shapes)
                    {
                        var shapeNode = CreateShapeNode(shape);
                        layerNode.Nodes.Add(shapeNode);
                    }
                    layerNode.Expand();
                }
            }

            // Restore selection
            if (selectedTag != null)
            {
                SelectNodeByTag(selectedTag);
            }
        }
        finally
        {
            EndUpdate();
        }
    }

    private TreeNode CreateLayerNode(Layer layer)
    {
        var layerType = layer switch
        {
            ShapeLayer sl => $"Shape Layer ({sl.Shapes.Count})",
            PaintLayer => "Paint Layer",
            _ => "Layer"
        };

        var text = string.IsNullOrEmpty(layer.Name) ? layerType : layer.Name;
        var node = new TreeNode(text)
        {
            Tag = layer
        };

        return node;
    }

    private TreeNode CreateShapeNode(Shape shape)
    {
        var shapeType = shape.GetType().Name;
        var text = string.IsNullOrEmpty(shape.Name) ? shapeType : $"{shape.Name} ({shapeType})";
        var node = new TreeNode(text)
        {
            Tag = shape
        };

        return node;
    }

    private void SelectNodeByTag(object tag)
    {
        foreach (TreeNode node in Nodes)
        {
            if (node.Tag == tag)
            {
                SelectedNode = node;
                return;
            }

            foreach (TreeNode childNode in node.Nodes)
            {
                if (childNode.Tag == tag)
                {
                    SelectedNode = childNode;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Get the selected layer (or parent layer if a shape is selected).
    /// </summary>
    public Layer? GetSelectedLayer()
    {
        if (SelectedNode?.Tag is Layer layer)
        {
            return layer;
        }
        if (SelectedNode?.Parent?.Tag is Layer parentLayer)
        {
            return parentLayer;
        }
        return null;
    }

    /// <summary>
    /// Get the selected shape (if any).
    /// </summary>
    public Shape? GetSelectedShape()
    {
        return SelectedNode?.Tag as Shape;
    }

    protected override void OnDrawNode(DrawTreeNodeEventArgs e)
    {
        if (e.Node == null) return;

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var bounds = e.Bounds;
        var isSelected = (e.State & TreeNodeStates.Selected) != 0;
        var isLayer = e.Node.Tag is Layer;
        var isShape = e.Node.Tag is Shape;

        // Draw background
        var backColor = isSelected ? SystemColors.Highlight : BackColor;
        using (var backBrush = new SolidBrush(backColor))
        {
            g.FillRectangle(backBrush, bounds);
        }

        // Calculate positions
        var iconSize = 16;
        var iconX = bounds.X + (e.Node.Level * 20) + 20;
        var iconY = bounds.Y + (bounds.Height - iconSize) / 2;
        var textX = iconX + iconSize + 4;
        var textY = bounds.Y;

        // Draw expand/collapse button for layers with shapes
        if (e.Node.Nodes.Count > 0)
        {
            var buttonSize = 9;
            var buttonX = bounds.X + (e.Node.Level * 20) + 5;
            var buttonY = bounds.Y + (bounds.Height - buttonSize) / 2;
            var buttonRect = new Rectangle(buttonX, buttonY, buttonSize, buttonSize);

            using (var pen = new Pen(SystemColors.GrayText))
            {
                g.DrawRectangle(pen, buttonRect);

                // Draw minus
                var midY = buttonY + buttonSize / 2;
                g.DrawLine(pen, buttonX + 2, midY, buttonX + buttonSize - 2, midY);

                // Draw plus if collapsed
                if (!e.Node.IsExpanded)
                {
                    var midX = buttonX + buttonSize / 2;
                    g.DrawLine(pen, midX, buttonY + 2, midX, buttonY + buttonSize - 2);
                }
            }
        }

        // Draw icon
        var iconRect = new Rectangle(iconX, iconY, iconSize, iconSize);
        DrawItemIcon(g, iconRect, e.Node.Tag);

        // Draw visibility/lock indicators
        if (e.Node.Tag is Layer layer)
        {
            if (!layer.IsVisible)
            {
                using var dimBrush = new SolidBrush(Color.FromArgb(100, backColor));
                g.FillRectangle(dimBrush, bounds);
            }

            if (layer.IsLocked)
            {
                var lockRect = new Rectangle(iconRect.Right - 6, iconRect.Bottom - 6, 6, 6);
                using var lockBrush = new SolidBrush(Color.Orange);
                g.FillRectangle(lockBrush, lockRect);
            }
        }
        else if (e.Node.Tag is Shape shape)
        {
            if (!shape.IsVisible)
            {
                using var dimBrush = new SolidBrush(Color.FromArgb(100, backColor));
                g.FillRectangle(dimBrush, bounds);
            }

            if (shape.IsLocked)
            {
                var lockRect = new Rectangle(iconRect.Right - 6, iconRect.Bottom - 6, 6, 6);
                using var lockBrush = new SolidBrush(Color.Orange);
                g.FillRectangle(lockBrush, lockRect);
            }
        }

        // Draw text
        var textColor = isSelected ? SystemColors.HighlightText : ForeColor;
        var textRect = new Rectangle(textX, textY, bounds.Right - textX, bounds.Height);

        using (var textBrush = new SolidBrush(textColor))
        {
            var format = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(e.Node.Text, Font, textBrush, textRect, format);
        }

        // Draw focus rectangle
        if ((e.State & TreeNodeStates.Focused) != 0)
        {
            ControlPaint.DrawFocusRectangle(g, bounds, textColor, backColor);
        }
    }

    private void DrawItemIcon(Graphics g, Rectangle bounds, object? tag)
    {
        if (tag is ShapeLayer)
        {
            // Draw shape layer icon (overlapping rectangles)
            using var brush = new SolidBrush(ShapeLayerColor);
            using var pen = new Pen(Color.FromArgb(80, 120, 160), 1);

            var rect1 = new Rectangle(bounds.X + 1, bounds.Y + 1, 8, 8);
            var rect2 = new Rectangle(bounds.X + 5, bounds.Y + 5, 8, 8);

            g.FillRectangle(brush, rect1);
            g.DrawRectangle(pen, rect1);
            g.FillRectangle(brush, rect2);
            g.DrawRectangle(pen, rect2);
        }
        else if (tag is PaintLayer)
        {
            // Draw paint layer icon
            using var brush = new SolidBrush(PaintLayerColor);
            g.FillEllipse(brush, bounds.X + 2, bounds.Y + 2, bounds.Width - 4, bounds.Height - 4);

            using var pen = new Pen(Color.FromArgb(120, 160, 80), 1.5f);
            var points = new PointF[]
            {
                new(bounds.X + 3, bounds.Y + bounds.Height / 2),
                new(bounds.X + bounds.Width / 2, bounds.Y + 4),
                new(bounds.X + bounds.Width - 3, bounds.Y + bounds.Height / 2)
            };
            g.DrawCurve(pen, points, 0.5f);
        }
        else if (tag is Shape shape)
        {
            // Draw shape-specific icon
            using var brush = new SolidBrush(ShapeColor);
            using var pen = new Pen(Color.FromArgb(120, 120, 180), 1);

            switch (shape)
            {
                case LineSegment:
                    g.DrawLine(pen, bounds.X + 2, bounds.Bottom - 2, bounds.Right - 2, bounds.Y + 2);
                    break;
                case Curve:
                    var curvePoints = new PointF[]
                    {
                        new(bounds.X + 2, bounds.Bottom - 2),
                        new(bounds.X + bounds.Width / 2, bounds.Y + 2),
                        new(bounds.Right - 2, bounds.Bottom - 2)
                    };
                    g.DrawCurve(pen, curvePoints, 0.5f);
                    break;
                case Circle:
                    g.FillEllipse(brush, bounds.X + 2, bounds.Y + 2, bounds.Width - 4, bounds.Height - 4);
                    g.DrawEllipse(pen, bounds.X + 2, bounds.Y + 2, bounds.Width - 4, bounds.Height - 4);
                    break;
                case Polygon:
                    g.FillRectangle(brush, bounds.X + 2, bounds.Y + 2, bounds.Width - 4, bounds.Height - 4);
                    g.DrawRectangle(pen, bounds.X + 2, bounds.Y + 2, bounds.Width - 4, bounds.Height - 4);
                    break;
                default:
                    // Generic shape icon
                    g.FillRectangle(brush, bounds.X + 3, bounds.Y + 3, bounds.Width - 6, bounds.Height - 6);
                    g.DrawRectangle(pen, bounds.X + 3, bounds.Y + 3, bounds.Width - 6, bounds.Height - 6);
                    break;
            }
        }
        else
        {
            // Generic icon
            using var brush = new SolidBrush(Color.Gray);
            g.FillRectangle(brush, bounds.X + 3, bounds.Y + 3, bounds.Width - 6, bounds.Height - 6);
        }
    }
}
