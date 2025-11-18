using MintPlayer.Bacon.Layers;
using MintPlayer.Bacon.Shapes;

namespace MintPlayer.Bacon.Editor.Demo;

public partial class FrmDemo : Form
{
    private Bacon bacon = new();
    private BaconImage? CurrentImage => editor.Bacon?.Images.FirstOrDefault();

    public FrmDemo()
    {
        InitializeComponent();
        editor.Bacon = bacon;
    }

    private void FrmDemo_Load(object sender, EventArgs e)
    {
    }

    private void btnAddImage_Click(object sender, EventArgs e)
    {
        var img = new BaconImage { Width = 64, Height = 64 };
        img.Layers.Add(new BaconPaintLayer { Name = "Background" });
        img.Layers.Add(new BaconShapeLayer { Name = "Shapes" });
        bacon.Images.Add(img);
        editor.Bacon = bacon; // refresh
    }

    private void btnAddRect_Click(object sender, EventArgs e)
    {
        if (CurrentImage == null) return;
        var layer = CurrentImage.Layers.OfType<BaconShapeLayer>().FirstOrDefault();
        if (layer == null) return;
        layer.Shapes.Add(new BaconRectangle
        {
            Rectangle = new Rectangle(10, 10, 20, 20),
            Pen = new Pen(Color.DarkBlue, 2),
            Brush = new SolidBrush(Color.FromArgb(128, Color.LightBlue)),
        });
        editor.Invalidate();
    }

    private void btnAddEllipse_Click(object sender, EventArgs e)
    {
        if (CurrentImage == null) return;
        var layer = CurrentImage.Layers.OfType<BaconShapeLayer>().FirstOrDefault();
        if (layer == null) return;
        layer.Shapes.Add(new BaconEllipse
        {
            Bounds = new Rectangle(30, 30, 24, 18),
            Pen = new Pen(Color.DarkGreen, 2),
            Brush = new SolidBrush(Color.FromArgb(128, Color.LightGreen)),
        });
        editor.Invalidate();
    }

    private void btnAddPolygon_Click(object sender, EventArgs e)
    {
        if (CurrentImage == null) return;
        var layer = CurrentImage.Layers.OfType<BaconShapeLayer>().FirstOrDefault();
        if (layer == null) return;
        layer.Shapes.Add(new BaconPolygon
        {
            Points = new List<Point> { new Point(40, 10), new Point(55, 40), new Point(25, 40) },
            Pen = new Pen(Color.Maroon, 2),
            Brush = new SolidBrush(Color.FromArgb(128, Color.OrangeRed)),
        });
        editor.Invalidate();
    }

    private void btnExportIco_Click(object sender, EventArgs e)
    {
        if (!bacon.Images.Any()) return;
        using var sfd = new SaveFileDialog { Filter = "Icon|*.ico" };
        if (sfd.ShowDialog(this) == DialogResult.OK)
        {
            using var icon = bacon.ToIcon();
            using var fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
            icon.Save(fs);
        }
    }
}
