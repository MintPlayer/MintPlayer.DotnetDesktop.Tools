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
    private int lastDxModelTotal;
    private int lastDyModelTotal;
    private Point initialMouseView;
    private List<Point>? initialShapeControlPoints; // initial control points for drag

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

        // Draw layers directly so shapes/glyphs outside bounds remain visible
        e.Graphics.ScaleTransform((float)zoom, (float)zoom);
        foreach (var layer in currentImage.Layers.Where(l => l.Visible))
        {
            layer.Draw(e.Graphics);
        }
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
                        dragShape = shape;
                        dragControlPointIndex = cp.idx;
                        dragging = true;
                        dragOffset = e.Location; // legacy offset (kept for whole-shape drag)
                        initialMouseView = e.Location;
                        initialShapeControlPoints = shape.GetControlPoints().ToList();
                        lastDxModelTotal = 0; lastDyModelTotal = 0;
                        shape.Selected = true;
                        canvasPanel.Invalidate();
                        return;
                        }
                    }
                    if (shape.HitTest(modelPoint))
                    {
                        dragShape = shape;
                        dragging = true;
                        dragOffset = e.Location; // legacy
                        initialMouseView = e.Location;
                        initialShapeControlPoints = shape.GetControlPoints().ToList();
                        lastDxModelTotal = 0; lastDyModelTotal = 0;
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
        var dxViewTotal = e.X - initialMouseView.X;
        var dyViewTotal = e.Y - initialMouseView.Y;
        var dxModelTotal = (int)Math.Round(dxViewTotal / zoom);
        var dyModelTotal = (int)Math.Round(dyViewTotal / zoom);
        // Only update if movement changed to reduce flicker
        if (dxModelTotal != lastDxModelTotal || dyModelTotal != lastDyModelTotal)
        {
            if (dragControlPointIndex >= 0 && initialShapeControlPoints is not null)
            {
                var original = initialShapeControlPoints[dragControlPointIndex];
                dragShape.SetControlPoint(dragControlPointIndex, new Point(original.X + dxModelTotal, original.Y + dyModelTotal));
            }
            else if (initialShapeControlPoints is not null)
            {
                // Move all control points relative to initial positions
                for (int i = 0; i < initialShapeControlPoints.Count; i++)
                {
                    var orig = initialShapeControlPoints[i];
                    dragShape.SetControlPoint(i, new Point(orig.X + dxModelTotal, orig.Y + dyModelTotal));
                }
            }
            lastDxModelTotal = dxModelTotal;
            lastDyModelTotal = dyModelTotal;
            canvasPanel.Invalidate();
        }
    }

    private void CanvasPanel_MouseUp(object? sender, MouseEventArgs e)
    {
        dragging = false;
        dragShape = null;
        dragControlPointIndex = -1;
        initialShapeControlPoints = null;
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
