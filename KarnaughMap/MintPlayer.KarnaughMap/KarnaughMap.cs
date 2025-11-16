using MintPlayer.KarnaughMap.Events.EventArgs;
using MintPlayer.KarnaughMap.Events.EventHandlers;
using MintPlayer.KarnaughMap.Exceptions;
using MintPlayer.KarnaughMap.Helpers;
using MintPlayer.QuineMcCluskey.Abstractions; // abstractions
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;

namespace MintPlayer.KarnaughMap;

[ToolboxItem(true)]
[DesignerSerializer(typeof(KarnaughMapSerializer), typeof(global::Microsoft.DotNet.DesignTools.Serialization.CodeDomSerializer))]
public partial class KarnaughMap : UserControl
{
    public KarnaughMap()
    {
        InitializeComponent();
        DoubleBuffered = true;

        //if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
        InputVariables = new ObservableCollection.ObservableCollection<string>();
        InputVariables.CollectionChanged += InputVariables_CollectionChanged;
        loops_ones = new ObservableCollection.ObservableCollection<IRequiredLoop>();
        loops_zeros = new ObservableCollection.ObservableCollection<IRequiredLoop>();

        EventHandler invalidateDelegate = (sender, e) => Invalidate();
        GotFocus += invalidateDelegate;
        LostFocus += invalidateDelegate;
    }

    // Solver injection
    private IQuineMcCluskeySolver solver;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IQuineMcCluskeySolver Solver { get => solver; set => solver = value; }

    #region Constants
    const int gridSize = 40;
    #endregion
    #region Private fields
    private string[] varsX;
    private string[] varsY;
    private int rowCount;
    private int columnCount;
    private Point focusedCell = new Point();
    private List<int> ones = new List<int>();
    private List<int> zeros = new List<int>();
    private List<int> selectedCells = new List<int>();
    private readonly MintPlayer.ObservableCollection.ObservableCollection<IRequiredLoop> loops_ones;
    private readonly MintPlayer.ObservableCollection.ObservableCollection<IRequiredLoop> loops_zeros;
    #endregion
    #region Private methods
    private int GridPositionToMinterm(Point gridIndex) => GridPositionToMinterm(gridIndex.X, gridIndex.Y);
    private int GridPositionToMinterm(int x, int y)
    {
        var x_gray = GrayCodeConverter.Decimal2Gray(x);
        var y_gray = GrayCodeConverter.Decimal2Gray(y);
        return y_gray * (1 << varsX.Length) + x_gray;
    }
    private Point MintermToGridPosition(int minterm)
    {
        var y_gray = minterm >> varsX.Length;
        var x_gray = minterm - (y_gray << varsX.Length);
        return new Point(GrayCodeConverter.Gray2Decimal(x_gray), GrayCodeConverter.Gray2Decimal(y_gray));
    }
    private void ToggleNumber(int minterm)
    {
        if (mode == Enums.EEditMode.Edit)
        {
            if (zeros.Contains(minterm))
            {
                if (ones.Contains(minterm))
                {
                    ones.Remove(minterm);
                    zeros.Remove(minterm);
                }
                else
                {
                    zeros.Remove(minterm);
                    ones.Add(minterm);
                }
            }
            else
            {
                if (ones.Contains(minterm)) zeros.Add(minterm); else zeros.Add(minterm);
            }
        }
        else
        {
            if (selectedCells.Contains(minterm)) selectedCells.RemoveAll(m => m == minterm); else selectedCells.Add(minterm);
        }
    }
    private void SetValue(int minterm, Enums.ECellValue value)
    {
        if (mode != Enums.EEditMode.Edit) return;
        switch (value)
        {
            case Enums.ECellValue.Zero:
                if (!zeros.Contains(minterm)) zeros.Add(minterm);
                if (ones.Contains(minterm)) ones.Remove(minterm);
                break;
            case Enums.ECellValue.One:
                if (zeros.Contains(minterm)) zeros.Remove(minterm);
                if (!ones.Contains(minterm)) ones.Add(minterm);
                break;
            case Enums.ECellValue.DontCare:
                if (!zeros.Contains(minterm)) zeros.Add(minterm);
                if (!ones.Contains(minterm)) ones.Add(minterm);
                break;
            case Enums.ECellValue.Undefined:
                if (zeros.Contains(minterm)) zeros.Remove(minterm);
                if (ones.Contains(minterm)) ones.Remove(minterm);
                break;
        }
    }
    private async Task<List<int>> CalculateRandomNumbers(int max)
    {
        var random = new Random();
        var list = new List<int>();
        await Task.Run(() =>
        {
            for (int i = 0; i < max; i++)
            {
                var num = random.Next(max);
                if (!list.Contains(num)) list.Add(num);
            }
        }).ConfigureAwait(false);
        return list;
    }
    #endregion
    #region Events
    public event EventHandler<KarnaughMapSolvedEventArgs> KarnaughMapSolved;
    public event EventHandler<KarnaughLoopAddedEventArgs> KarnaughLoopAdded;
    #endregion
    #region Public methods
    public async Task RandomFill()
    {
        if (mode != Enums.EEditMode.Edit) return;
        ones = await CalculateRandomNumbers(1 << InputVariables.Count).ConfigureAwait(false);
        zeros = await CalculateRandomNumbers(1 << InputVariables.Count).ConfigureAwait(false);
        Invalidate();
    }
    public async Task SolveSelection()
    {
        try
        {
            if (mode != Enums.EEditMode.Solve) return;
            if (solver == null) throw new InvalidOperationException("Solver not set for KarnaughMap.");
            SuspendLayout();
            if (!selectedCells.Any()) throw new MinificationException("Please select some cells to join.");
            var selected_ones = ones.Except(zeros).Intersect(selectedCells);
            var selected_zeros = zeros.Except(ones).Intersect(selectedCells);
            var selected_dontcares = zeros.Intersect(ones).Intersect(selectedCells);
            bool value;
            if (selected_ones.Any())
            {
                if (selected_zeros.Any()) throw new MinificationException("Selected minterms must have the same value.");
                value = true;
            }
            else
            {
                if (selected_zeros.Any()) value = false; else throw new MinificationException("Selected minterms cannot all be don't cares.");
            }
            var res = await solver.QMC_Solve(value ? selected_ones : selected_zeros, selected_dontcares);
            var result = res.ToList();
            if (result.Count != 1) throw new MinificationException("Selected minterms cannot be simplified.");
            if (value) loops_ones.Add(result.First()); else loops_zeros.Add(result.First());
            selectedCells.Clear();
            KarnaughLoopAdded?.Invoke(this, new KarnaughLoopAddedEventArgs(result.First(), value));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            ResumeLayout(true);
            Invalidate();
        }
    }
    public async Task SolveAutomatically()
    {
        try
        {
            if (mode != Enums.EEditMode.Solve) return;
            if (solver == null) throw new InvalidOperationException("Solver not set for KarnaughMap.");
            SuspendLayout();
            var dontcares = ones.Intersect(zeros);
            var solved_loops_ones = (await solver.QMC_Solve(ones, dontcares)).ToList();
            var solved_loops_zeros = (await solver.QMC_Solve(zeros, dontcares)).ToList();
            loops_ones.Clear(); loops_ones.AddRange(solved_loops_ones);
            loops_zeros.Clear(); loops_zeros.AddRange(solved_loops_zeros);
            KarnaughMapSolved?.Invoke(this, new KarnaughMapSolvedEventArgs(solved_loops_ones, solved_loops_zeros));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            ResumeLayout(true);
            Invalidate();
        }
    }
    #endregion
    #region Properties
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public MintPlayer.ObservableCollection.ObservableCollection<string> InputVariables { get; private set; }
    private string outputVariable;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string OutputVariable { get => outputVariable; set { outputVariable = value; Invalidate(); } }
    private Enums.EEditMode mode;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Enums.EEditMode Mode
    {
        get => mode;
        set
        {
            var args = new ModeChangingEventArgs(mode, value);
            ModeChanging?.Invoke(this, args);
            if (args.Cancel) return;
            mode = value;
            if (mode == Enums.EEditMode.Edit)
            {
                loops_ones.Clear();
                loops_zeros.Clear();
                selectedCells.Clear();
            }
            Invalidate();
        }
    }
    public event ModeChangingEventHandler ModeChanging;
    public bool HasLoops => loops_ones.Any() & loops_zeros.Any();
    private IRequiredLoop selected_loop;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IRequiredLoop SelectedLoop
    {
        get => selected_loop;
        set
        {
            selected_loop = value;
            selectedCells = selected_loop == null ? new List<int>() : value.MinTerms.ToList();
            Invalidate();
        }
    }
    #endregion
    #region Event handlers
    private void InputVariables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        var varsYcount = InputVariables.Count >> 1;
        var varsXcount = InputVariables.Count - varsYcount;
        varsY = InputVariables.Take(varsYcount).ToArray();
        varsX = InputVariables.Skip(varsYcount).ToArray();
        rowCount = 1 << varsYcount;
        columnCount = 1 << varsXcount;
        Invalidate();
    }
    private void KarnaughMap_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Left:
                if (--focusedCell.X < 0) focusedCell.X = columnCount - 1; break;
            case Keys.Right:
                if (++focusedCell.X >= columnCount) focusedCell.X = 0; break;
            case Keys.Up:
                if (--focusedCell.Y < 0) focusedCell.Y = rowCount - 1; break;
            case Keys.Down:
                if (++focusedCell.Y >= rowCount) focusedCell.Y = 0; break;
            case Keys.Space:
                ToggleNumber(GridPositionToMinterm(focusedCell)); break;
            case Keys.D0:
            case Keys.NumPad0:
                SetValue(GridPositionToMinterm(focusedCell), Enums.ECellValue.Zero); break;
            case Keys.D1:
            case Keys.NumPad1:
                SetValue(GridPositionToMinterm(focusedCell), Enums.ECellValue.One); break;
            case Keys.X:
                SetValue(GridPositionToMinterm(focusedCell), Enums.ECellValue.DontCare); break;
            case Keys.OemMinus:
                SetValue(GridPositionToMinterm(focusedCell), Enums.ECellValue.Undefined); break;
            default: return;
        }
        Invalidate();
    }
    private void KarnaughMap_Paint(object sender, PaintEventArgs e)
    {
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        Width = (columnCount + 1) * gridSize + 1 + Font.Height;
        Height = (rowCount + 1) * gridSize + 1 + Font.Height;
        e.Graphics.Clear(BackColor);
        e.Graphics.DrawString(OutputVariable, Font, Brushes.Black, 0, 0);
        e.Graphics.TranslateTransform(Font.Height, Font.Height);
        e.Graphics.DrawString(string.Join(", ", varsX), Font, Brushes.Black, new RectangleF(gridSize, -Font.Height, columnCount * gridSize, Font.Height), sf);
        for (int i = 0; i <= columnCount; i++) e.Graphics.DrawLine(Pens.Black, gridSize * (i + 1), gridSize, gridSize * (i + 1), gridSize * (rowCount + 1));
        for (int j = 0; j <= rowCount; j++) e.Graphics.DrawLine(Pens.Black, gridSize, gridSize * (j + 1), gridSize * (columnCount + 1), gridSize * (j + 1));
        e.Graphics.DrawLine(Pens.Black, 0, 0, gridSize, gridSize);
        for (int i = 0; i < rowCount; i++)
        {
            var gray = GrayCodeConverter.Decimal2Gray(i);
            e.Graphics.DrawString(Convert.ToString(gray, 2).PadLeft(varsY.Length, '0'), Font, Brushes.Black, new RectangleF(0, (i + 1) * gridSize, gridSize, gridSize), sf);
        }
        var grid_transform = e.Graphics.Transform;
        e.Graphics.RotateTransform(-90);
        e.Graphics.DrawString(string.Join(", ", varsY), Font, Brushes.Black, new RectangleF(-Height + Font.Height, -Font.Height, Height - Font.Height - gridSize, Font.Height), sf);
        for (int i = 0; i < columnCount; i++)
        {
            var gray = GrayCodeConverter.Decimal2Gray(i);
            e.Graphics.DrawString(Convert.ToString(gray, 2).PadLeft(varsX.Length, '0'), Font, Brushes.Black, new RectangleF(-gridSize, (i + 1) * gridSize, gridSize, gridSize), sf);
        }
        e.Graphics.Transform = grid_transform;
        foreach (var minterm in loops_ones.SelectMany(l => l.MinTerms).Distinct())
        {
            var pos = MintermToGridPosition(minterm);
            var br = zeros.Contains(minterm) ? new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.Percent50, Color.Olive, Color.Transparent) : Brushes.Olive;
            e.Graphics.FillRectangle(br, (pos.X + 1) * gridSize + 1, (pos.Y + 1) * gridSize + 1, gridSize - 1, gridSize - 1);
        }
        foreach (var minterm in loops_zeros.SelectMany(l => l.MinTerms).Distinct())
        {
            var pos = MintermToGridPosition(minterm);
            var br = ones.Contains(minterm) ? new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.Percent50, Color.Transparent, Color.OrangeRed) : Brushes.OrangeRed;
            e.Graphics.FillRectangle(br, (pos.X + 1) * gridSize + 1, (pos.Y + 1) * gridSize + 1, gridSize - 1, gridSize - 1);
        }
        if (mode == Enums.EEditMode.Solve)
        {
            foreach (var minterm in selectedCells)
            {
                var pos = MintermToGridPosition(minterm);
                e.Graphics.FillRectangle(Brushes.Yellow, (pos.X + 1) * gridSize + 1, (pos.Y + 1) * gridSize + 1, gridSize - 1, gridSize - 1);
            }
        }
        for (int i = 0; i < columnCount; i++)
        {
            for (int j = 0; j < rowCount; j++)
            {
                var index = GridPositionToMinterm(i, j);
                string text = ones.Contains(index) ? (zeros.Contains(index) ? "X" : "1") : (zeros.Contains(index) ? "0" : "-");
                e.Graphics.DrawString(text, Font, Brushes.Black, new RectangleF((i + 1) * gridSize, (j + 1) * gridSize, gridSize, gridSize), sf);
            }
        }
        if (Focused)
        {
            var rct = new Rectangle((focusedCell.X + 1) * gridSize, (focusedCell.Y + 1) * gridSize, gridSize, gridSize);
            rct.Inflate(-2, -2);
            using var pen = new Pen(Color.Black, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };
            e.Graphics.DrawRectangle(pen, rct);
        }
    }
    private void KarnaughMap_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.X < gridSize + Font.Height) return;
        if (e.Y < gridSize + Font.Height) return;
        var x = (e.X - gridSize - Font.Height) / gridSize;
        var y = (e.Y - gridSize - Font.Height) / gridSize;
        var index = GridPositionToMinterm(x, y);
        focusedCell = new Point(x, y);
        ToggleNumber(index);
        Invalidate();
    }
    #endregion
    protected override bool IsInputKey(Keys keyData)
    {
        return keyData is Keys.Left or Keys.Right or Keys.Up or Keys.Down ? true : base.IsInputKey(keyData);
    }
}
