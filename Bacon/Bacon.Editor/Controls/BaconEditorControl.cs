using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MintPlayer.Bacon.Editor.Controls;

public class BaconEditorControl : UserControl
{
    private ListBox lbImages = new();
    private Panel canvasPanel = new();
    private CheckedListBox clbLayers = new();
    private Bacon.Bacon? bacon;
    private Bacon.BaconImage? currentImage;

    public BaconEditorControl()
    {
        DoubleBuffered = true;
        lbImages.Dock = DockStyle.Left;
        lbImages.Width = 120;
        lbImages.SelectedIndexChanged += (s, e) => SelectImage(lbImages.SelectedIndex);

        clbLayers.Dock = DockStyle.Right;
        clbLayers.Width = 150;
        clbLayers.ItemCheck += (s, e) =>
        {
            if (currentImage == null) return;
            var layer = currentImage.Layers[e.Index];
            layer.Visible = e.NewValue == CheckState.Checked;
            canvasPanel.Invalidate();
        };

        canvasPanel.Dock = DockStyle.Fill;
        canvasPanel.Paint += CanvasPanel_Paint;
        canvasPanel.MouseDown += CanvasPanel_MouseDown;
        canvasPanel.MouseMove += CanvasPanel_MouseMove;
        canvasPanel.MouseUp += CanvasPanel_MouseUp;

        Controls.Add(canvasPanel);
        Controls.Add(clbLayers);
        Controls.Add(lbImages);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Bacon.Bacon? Bacon
    {
        get => bacon;
        set
        {
            bacon = value;
            lbImages.Items.Clear();
            if (bacon != null)
            {
                foreach (var img in bacon.Images)
                {
                    lbImages.Items.Add($"{img.Width}x{img.Height}");
                }
            }
            SelectImage(0);
        }
    }

    private void SelectImage(int index)
    {
        if (bacon == null || index < 0 || index >= bacon.Images.Count)
        {
            currentImage = null;
            clbLayers.Items.Clear();
            canvasPanel.Invalidate();
            return;
        }
        currentImage = bacon.Images[index];
        clbLayers.Items.Clear();
        for (int i = 0; i < currentImage.Layers.Count; i++)
        {
            clbLayers.Items.Add(currentImage.Layers[i].Name, currentImage.Layers[i].Visible);
        }
        canvasPanel.Invalidate();
    }

    private bool dragging;
    private Bacon.Shapes.BaconShape? dragShape;
    private Point dragOffset;
    private int dragControlPointIndex = -1;

    private void CanvasPanel_Paint(object? sender, PaintEventArgs e)
    {
        e.Graphics.Clear(SystemColors.Window);
        if (currentImage == null) return;
        using var bmp = currentImage.Render();
        e.Graphics.DrawImage(bmp, 0, 0);
    }

    private void CanvasPanel_MouseDown(object? sender, MouseEventArgs e)
    {
        if (currentImage == null) return;
        foreach (var layer in currentImage.Layers)
        {
            if (!layer.Visible) continue;
            if (layer is Bacon.Layers.BaconShapeLayer sl)
            {
                foreach (var shape in sl.Shapes)
                {
                    // Check control points first
                    var cps = shape.GetControlPoints().Select((pt, idx) => (pt, idx)).ToList();
                    foreach (var cp in cps)
                    {
                        var r = new Rectangle(cp.pt.X - 4, cp.pt.Y - 4, 8, 8);
                        if (r.Contains(e.Location))
                        {
                            dragShape = shape; dragControlPointIndex = cp.idx; dragging = true; dragOffset = e.Location; shape.Selected = true; canvasPanel.Invalidate(); return;
                        }
                    }
                    if (shape.HitTest(e.Location))
                    {
                        dragShape = shape;
                        dragging = true;
                        dragOffset = e.Location;
                        dragControlPointIndex = -1;
                        shape.Selected = true;
                        return;
                    }
                }
            }
        }
    }

    private void CanvasPanel_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!dragging || dragShape == null) return;
        var dx = e.X - dragOffset.X;
        var dy = e.Y - dragOffset.Y;
        if (dragControlPointIndex >= 0)
        {
            var cp = dragShape.GetControlPoints().ElementAt(dragControlPointIndex);
            dragShape.SetControlPoint(dragControlPointIndex, new Point(cp.X + dx, cp.Y + dy));
        }
        else
        {
            dragShape.Offset(dx, dy);
        }
        dragOffset = e.Location;
        canvasPanel.Invalidate();
    }

    private void CanvasPanel_MouseUp(object? sender, MouseEventArgs e)
    {
        dragging = false;
        dragShape = null;
        dragControlPointIndex = -1;
    }
}
