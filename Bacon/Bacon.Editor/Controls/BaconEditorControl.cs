using MintPlayer.Bacon.Layers; // Layers
using MintPlayer.Bacon.Shapes; // Shapes
using System.ComponentModel;

namespace MintPlayer.Bacon.Editor.Controls;

public class BaconEditorControl : UserControl
{
    private ListBox lbImages = new();
    private Panel canvasPanel = new();
    private CheckedListBox clbLayers = new();
    private Bacon? bacon;
    private BaconImage? currentImage;

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
            if (currentImage is null) return;
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
    public Bacon? Bacon
    {
        get => bacon;
        set
        {
            bacon = value;
            lbImages.Items.Clear();
            if (bacon is not null)
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
        if (bacon is null || index < 0 || index >= bacon.Images.Count)
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
    private BaconShape? dragShape;
    private Point dragOffset;
    private int dragControlPointIndex = -1;
    private double zoom = 1.0;
    private const double zoomMin = 0.25;
    private const double zoomMax = 8.0;
    private const double zoomStep = 0.1;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double Zoom
    {
        get => zoom;
        set
        {
            zoom = Math.Max(zoomMin, Math.Min(zoomMax, value));
            canvasPanel.Invalidate();
        }
    }

    private void CanvasPanel_Paint(object? sender, PaintEventArgs e)
    {
        e.Graphics.Clear(SystemColors.Window);
        if (currentImage is null) return;

        // Draw tinted background for image boundary
        var imgWidth = (int)(currentImage.Width * zoom);
        var imgHeight = (int)(currentImage.Height * zoom);
        using (var bgBrush = new SolidBrush(Color.FromArgb(235, 240, 250)))
            e.Graphics.FillRectangle(bgBrush, new Rectangle(0, 0, imgWidth, imgHeight));
        using (var hatch = new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.LightDownwardDiagonal, Color.FromArgb(220, 230, 240), Color.FromArgb(235, 240, 250)))
            e.Graphics.FillRectangle(hatch, new Rectangle(0, 0, imgWidth, imgHeight));
        using (var borderPen = new Pen(Color.SteelBlue))
            e.Graphics.DrawRectangle(borderPen, new Rectangle(0, 0, imgWidth - 1, imgHeight - 1));

        e.Graphics.TranslateTransform(0, 0);
        e.Graphics.ScaleTransform((float)zoom, (float)zoom);
        using var bmp = currentImage.Render();
        e.Graphics.DrawImage(bmp, 0, 0);
    }

    private void CanvasPanel_MouseDown(object? sender, MouseEventArgs e)
    {
        if (currentImage is null) return;
        var modelPoint = new Point((int)Math.Round(e.Location.X / zoom), (int)Math.Round(e.Location.Y / zoom));
        foreach (var layer in currentImage.Layers)
        {
            if (!layer.Visible) continue;
            if (layer is BaconShapeLayer sl)
            {
                foreach (var shape in sl.Shapes)
                {
                    // Check control points first
                    var cps = shape.GetControlPoints().Select((pt, idx) => (pt, idx)).ToList();
                    foreach (var cp in cps)
                    {
                        var r = new Rectangle(cp.pt.X - 4, cp.pt.Y - 4, 8, 8);
                        if (r.Contains(modelPoint))
                        {
                            dragShape = shape; dragControlPointIndex = cp.idx; dragging = true; dragOffset = e.Location; shape.Selected = true; canvasPanel.Invalidate(); return;
                        }
                    }
                    if (shape.HitTest(modelPoint))
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
        if (!dragging || dragShape is null) return;
        var dxView = e.X - dragOffset.X;
        var dyView = e.Y - dragOffset.Y;
        var dx = (int)Math.Round(dxView / zoom);
        var dy = (int)Math.Round(dyView / zoom);
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

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if ((ModifierKeys & Keys.Control) == Keys.Control)
        {
            double oldZoom = zoom;
            if (e.Delta > 0) Zoom += zoomStep; else Zoom -= zoomStep;
            // Optional: keep point under cursor stable (pan not implemented yet)
        }
    }
}
