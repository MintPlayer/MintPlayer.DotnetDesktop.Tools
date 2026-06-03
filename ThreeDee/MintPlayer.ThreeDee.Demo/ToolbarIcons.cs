using System.Drawing.Drawing2D;

namespace MintPlayer.ThreeDee;

/// <summary>
/// Vector toolbar/menu icons drawn with GDI+ at 16×16, so the demo ships no external image
/// assets. Glyphs are cached by key. Colours are tuned for the default (light) ToolStrip theme.
/// </summary>
internal static class ToolbarIcons
{
    const int S = 16;
    static readonly Color Ink = Color.FromArgb(0x3a, 0x42, 0x4e);
    static readonly Color Accent = Color.FromArgb(0x2f, 0x7f, 0xd6);
    static readonly Color Ghost = Color.FromArgb(0x9a, 0xb0, 0xc8);
    static readonly Color Danger = Color.FromArgb(0xc6, 0x46, 0x46);
    static readonly Color Fill = Color.FromArgb(0xcf, 0xe2, 0xf6);

    static readonly Dictionary<string, Bitmap> _cache = new();

    public static Bitmap Get(string key)
    {
        if (_cache.TryGetValue(key, out var b)) return b;
        var bmp = new Bitmap(S, S);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(0.5f, 0.5f); // crisp 1px strokes
            Draw(key, g);
        }
        _cache[key] = bmp;
        return bmp;
    }

    static Pen P(Color c, float w = 1.4f) => new(c, w) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
    static PointF Pt(float x, float y) => new(x, y);

    static void Arrow(Graphics g, Pen p, PointF from, PointF to, float head = 3.2f)
    {
        g.DrawLine(p, from, to);
        double a = Math.Atan2(to.Y - from.Y, to.X - from.X);
        foreach (int s in new[] { -1, 1 })
        {
            double b = a + s * 2.6; // ~150°
            g.DrawLine(p, to, Pt(to.X + (float)(Math.Cos(b) * head), to.Y + (float)(Math.Sin(b) * head)));
        }
    }

    // direction: 'L' 'R' 'U' 'D'
    static (PointF from, PointF to) Dir(char d) => d switch
    {
        'L' => (Pt(12, 8), Pt(4, 8)),
        'R' => (Pt(4, 8), Pt(12, 8)),
        'U' => (Pt(8, 12), Pt(8, 4)),
        _ => (Pt(8, 4), Pt(8, 12)),
    };

    static void Cube(Graphics g, Pen ink, bool fill)
    {
        PointF[] top = { Pt(8, 1.5f), Pt(13.5f, 5), Pt(8, 8.5f), Pt(2.5f, 5) };
        PointF[] left = { Pt(2.5f, 5), Pt(8, 8.5f), Pt(8, 14.5f), Pt(2.5f, 11) };
        PointF[] right = { Pt(13.5f, 5), Pt(8, 8.5f), Pt(8, 14.5f), Pt(13.5f, 11) };
        if (fill)
        {
            using var fT = new SolidBrush(Fill);
            using var fL = new SolidBrush(Color.FromArgb(0xb4, 0xcc, 0xe8));
            using var fR = new SolidBrush(Color.FromArgb(0xa0, 0xbc, 0xdc));
            g.FillPolygon(fT, top); g.FillPolygon(fL, left); g.FillPolygon(fR, right);
        }
        g.DrawPolygon(ink, top); g.DrawPolygon(ink, left); g.DrawPolygon(ink, right);
    }

    static void Magnifier(Graphics g, Pen ink)
    {
        g.DrawEllipse(ink, 2, 2, 8, 8);
        g.DrawLine(P(ink.Color, 2f), 9.3f, 9.3f, 14, 14);
    }

    static void Draw(string key, Graphics g)
    {
        using var ink = P(Ink);
        using var accent = P(Accent);
        using var ghost = P(Ghost, 1.2f);
        using var danger = P(Danger);

        switch (key)
        {
            // ---- file ----
            case "new":
                g.DrawPolygon(ink, new[] { Pt(3, 1.5f), Pt(10, 1.5f), Pt(13, 4.5f), Pt(13, 14.5f), Pt(3, 14.5f) });
                g.DrawPolygon(ink, new[] { Pt(10, 1.5f), Pt(10, 4.5f), Pt(13, 4.5f) });
                break;
            case "open":
                g.DrawPolygon(ink, new[] { Pt(1.5f, 4), Pt(6, 4), Pt(7.5f, 6), Pt(14.5f, 6), Pt(14.5f, 13), Pt(1.5f, 13) });
                g.DrawLine(accent, 1.5f, 13, 4, 8); g.DrawLine(accent, 14.5f, 13, 12, 8); g.DrawLine(accent, 4, 8, 12, 8);
                break;
            case "save":
                g.DrawPolygon(ink, new[] { Pt(2.5f, 2.5f), Pt(11.5f, 2.5f), Pt(13.5f, 4.5f), Pt(13.5f, 13.5f), Pt(2.5f, 13.5f) });
                g.DrawRectangle(ink, 5, 2.5f, 5, 3.5f);
                g.DrawRectangle(accent, 5, 9, 6, 4.5f);
                break;
            case "saveas":
                g.DrawPolygon(ink, new[] { Pt(2.5f, 2.5f), Pt(10.5f, 2.5f), Pt(13.5f, 5.5f), Pt(13.5f, 13.5f), Pt(2.5f, 13.5f) });
                g.DrawRectangle(ink, 4.5f, 2.5f, 4.5f, 3.5f);
                Arrow(g, accent, Pt(7, 12), Pt(12.5f, 12)); g.DrawLine(accent, 9, 9.5f, 12.5f, 9.5f);
                break;
            case "export":
                g.DrawRectangle(ink, 2.5f, 2.5f, 11, 11);
                Arrow(g, accent, Pt(8, 11), Pt(8, 4.5f));
                break;
            case "exit":
                g.DrawPolygon(ink, new[] { Pt(7, 2.5f), Pt(2.5f, 2.5f), Pt(2.5f, 13.5f), Pt(7, 13.5f) });
                Arrow(g, accent, Pt(6, 8), Pt(13.5f, 8)); g.DrawLine(accent, 10.5f, 5, 13.5f, 8); g.DrawLine(accent, 10.5f, 11, 13.5f, 8);
                break;

            // ---- edit ----
            case "undo":
                g.DrawArc(accent, 3, 4, 9, 9, 30, 250);
                Arrow(g, accent, Pt(5.5f, 2.5f), Pt(3.2f, 5.2f));
                break;
            case "redo":
                g.DrawArc(accent, 4, 4, 9, 9, -100, -250);
                Arrow(g, accent, Pt(10.5f, 2.5f), Pt(12.8f, 5.2f));
                break;
            case "selectall":
                g.DrawRectangle(ghost, 2.5f, 2.5f, 11, 11);
                using (var f = new SolidBrush(Fill)) g.FillRectangle(f, 5, 5, 6, 6);
                g.DrawRectangle(accent, 5, 5, 6, 6);
                break;
            case "clearsel":
                g.DrawRectangle(ghost, 2.5f, 2.5f, 11, 11);
                g.DrawLine(danger, 5, 5, 11, 11); g.DrawLine(danger, 11, 5, 5, 11);
                break;

            // ---- draw tools ----
            case "select":
                g.FillPolygon(new SolidBrush(Ink), new[] { Pt(3, 2), Pt(3, 12), Pt(6, 9.5f), Pt(8, 13.5f), Pt(9.7f, 12.8f), Pt(7.7f, 8.8f), Pt(11.5f, 8.5f) });
                break;
            case "line":
                g.DrawLine(P(Accent, 1.6f), 3, 13, 13, 3);
                Dot(g, 3, 13); Dot(g, 13, 3);
                break;
            case "rect":
                g.DrawRectangle(accent, 2.5f, 4.5f, 11, 7);
                break;
            case "circle":
                g.DrawEllipse(accent, 2.5f, 2.5f, 11, 11);
                break;
            case "arc":
                g.DrawArc(accent, 1, 4, 14, 16, 180, 180);
                Dot(g, 2, 12); Dot(g, 14, 12);
                break;
            case "bezier":
                using (var p = P(Accent, 1.5f))
                {
                    var pts = new[] { Pt(2, 13), Pt(2, 4), Pt(14, 12), Pt(14, 3) };
                    g.DrawBezier(p, pts[0], pts[1], pts[2], pts[3]);
                }
                Dot(g, 2, 13); Dot(g, 14, 3);
                break;
            case "pushpull":
                Cube(g, ink, false);
                Arrow(g, accent, Pt(8, 8), Pt(8, 1.5f));
                break;

            // ---- modify ----
            case "move":
                g.DrawLine(accent, 8, 2.5f, 8, 13.5f); g.DrawLine(accent, 2.5f, 8, 13.5f, 8);
                foreach (var d in "LRUD") { var (f, t) = Dir(d); Arrow(g, accent, Lerp(f, t, 0.35f), t, 2.6f); }
                break;
            case "rotate":
                g.DrawArc(accent, 3, 3, 10, 10, 70, 290);
                Arrow(g, accent, Pt(7.5f, 2.6f), Pt(11.2f, 4.0f));
                break;
            case "followme":
                using (var p = P(Accent, 1.5f)) g.DrawBezier(p, Pt(2, 13), Pt(4, 5), Pt(11, 11), Pt(14, 3));
                g.DrawRectangle(ink, 1, 11, 4, 4);
                break;
            case "eraser":
                g.DrawPolygon(ink, new[] { Pt(5, 13), Pt(2.5f, 10.5f), Pt(9, 4), Pt(13.5f, 8.5f), Pt(10, 13) });
                g.DrawLine(ink, 5, 13, 13.5f, 13);
                break;
            case "delete":
                g.DrawLine(danger, 3, 4, 13, 4);
                g.DrawPolygon(danger, new[] { Pt(4, 4), Pt(12, 4), Pt(11, 14), Pt(5, 14) });
                g.DrawLine(ink, 6.5f, 2.5f, 9.5f, 2.5f);
                g.DrawLine(P(Danger, 1f), 6.5f, 6.5f, 7, 11.5f); g.DrawLine(P(Danger, 1f), 9.5f, 6.5f, 9, 11.5f);
                break;
            case "tape":
                g.DrawRectangle(ink, 2.5f, 5.5f, 11, 5);
                for (float x = 4.5f; x < 13; x += 2) g.DrawLine(accent, x, 5.5f, x, 8);
                break;
            case "plane":
                g.DrawPolygon(accent, new[] { Pt(2.5f, 9), Pt(7, 5.5f), Pt(13.5f, 7), Pt(9, 10.5f) });
                g.DrawLine(ghost, 9, 10.5f, 9, 13.5f);
                break;

            // ---- views ----
            case "iso": Cube(g, ink, true); break;
            case "top":
                Cube(g, ghost, false);
                g.FillPolygon(new SolidBrush(Accent), new[] { Pt(8, 1.5f), Pt(13.5f, 5), Pt(8, 8.5f), Pt(2.5f, 5) });
                break;
            case "front":
                Cube(g, ghost, false);
                g.FillPolygon(new SolidBrush(Accent), new[] { Pt(2.5f, 5), Pt(8, 8.5f), Pt(8, 14.5f), Pt(2.5f, 11) });
                break;
            case "right":
                Cube(g, ghost, false);
                g.FillPolygon(new SolidBrush(Accent), new[] { Pt(13.5f, 5), Pt(8, 8.5f), Pt(8, 14.5f), Pt(13.5f, 11) });
                break;
            case "projection":
                g.DrawPolygon(ink, new[] { Pt(2, 4), Pt(6, 2.5f), Pt(6, 13.5f), Pt(2, 12) });
                g.DrawPolygon(accent, new[] { Pt(14, 5.5f), Pt(10, 4.5f), Pt(10, 11.5f), Pt(14, 10.5f) });
                g.DrawLine(ghost, 6, 4, 10, 5); g.DrawLine(ghost, 6, 12, 10, 11);
                break;

            // ---- styles ----
            case "wire":
                Cube(g, accent, false);
                g.DrawLine(ghost, 2.5f, 5, 8, 8.5f); g.DrawLine(ghost, 13.5f, 5, 8, 8.5f); g.DrawLine(ghost, 8, 8.5f, 8, 14.5f);
                break;
            case "hidden": Cube(g, ink, false); break;
            case "shaded":
                PointF[] sTop = { Pt(8, 1.5f), Pt(13.5f, 5), Pt(8, 8.5f), Pt(2.5f, 5) };
                PointF[] sLeft = { Pt(2.5f, 5), Pt(8, 8.5f), Pt(8, 14.5f), Pt(2.5f, 11) };
                PointF[] sRight = { Pt(13.5f, 5), Pt(8, 8.5f), Pt(8, 14.5f), Pt(13.5f, 11) };
                g.FillPolygon(new SolidBrush(Fill), sTop);
                g.FillPolygon(new SolidBrush(Color.FromArgb(0xb4, 0xcc, 0xe8)), sLeft);
                g.FillPolygon(new SolidBrush(Color.FromArgb(0xa0, 0xbc, 0xdc)), sRight);
                break;
            case "shadededges": Cube(g, ink, true); break;

            // ---- navigation ----
            case "orbitL": case "orbitR": case "orbitU": case "orbitD":
                g.DrawEllipse(ghost, 2.5f, 2.5f, 11, 11);
                { var (f, t2) = Dir(key[^1]); Arrow(g, accent, Lerp(f, t2, 0.15f), t2); }
                break;
            case "panL": case "panR": case "panU": case "panD":
                { var (f, t2) = Dir(key[^1]); Arrow(g, accent, f, t2); }
                break;
            case "zoomin":
                Magnifier(g, ink);
                g.DrawLine(accent, 6, 4, 6, 8); g.DrawLine(accent, 4, 6, 8, 6);
                break;
            case "zoomout":
                Magnifier(g, ink);
                g.DrawLine(accent, 4, 6, 8, 6);
                break;
            case "zoomext":
                g.DrawRectangle(ghost, 2.5f, 2.5f, 11, 11);
                Arrow(g, accent, Pt(8, 8), Pt(4, 4), 2.4f); Arrow(g, accent, Pt(8, 8), Pt(12, 4), 2.4f);
                Arrow(g, accent, Pt(8, 8), Pt(4, 12), 2.4f); Arrow(g, accent, Pt(8, 8), Pt(12, 12), 2.4f);
                break;

            case "help":
                g.DrawEllipse(ink, 2.5f, 2.5f, 11, 11);
                using (var font = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                using (var br = new SolidBrush(Accent))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("?", font, br, new RectangleF(2.5f, 2f, 11, 12), sf);
                }
                break;

            default:
                g.DrawRectangle(ghost, 3, 3, 10, 10);
                break;
        }
    }

    static void Dot(Graphics g, float x, float y)
    {
        using var b = new SolidBrush(Accent);
        g.FillEllipse(b, x - 1.4f, y - 1.4f, 2.8f, 2.8f);
    }

    static PointF Lerp(PointF a, PointF b, float t) => Pt(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
}
