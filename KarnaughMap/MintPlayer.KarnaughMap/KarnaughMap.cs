using MintPlayer.KarnaughMap.Enums;
using MintPlayer.KarnaughMap.Events.EventArgs;
using MintPlayer.KarnaughMap.Events.EventHandlers;
using MintPlayer.QuineMcCluskey.Abstractions;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;

namespace MintPlayer.KarnaughMap;

/// <summary>
/// An interactive Karnaugh map.
/// </summary>
/// <remarks>
/// The view half of an MVP split. Every decision - what a cell shows, which overlay it
/// carries, where the focus is, how big the control wants to be - lives in
/// <see cref="KarnaughMapPresenter"/> and arrives here as a <see cref="KarnaughRenderModel"/>.
/// What remains below is GDI+ that walks that model and holds no rules of its own.
/// </remarks>
[ToolboxItem(true)]
[Designer(typeof(KarnaughMapDesigner))]
[DesignerSerializer(typeof(KarnaughMapSerializer), typeof(global::Microsoft.DotNet.DesignTools.Serialization.CodeDomSerializer))]
public partial class KarnaughMap : UserControl, IKarnaughMapView
{
    private readonly KarnaughMapPresenter presenter;

    public KarnaughMap()
    {
        InitializeComponent();
        DoubleBuffered = true;

        presenter = new KarnaughMapPresenter(this);
        presenter.Solved += (_, e) => KarnaughMapSolved?.Invoke(this, new KarnaughMapSolvedEventArgs([.. e.Ones], [.. e.Zeros]));
        presenter.LoopAdded += (_, e) => KarnaughLoopAdded?.Invoke(this, new KarnaughLoopAddedEventArgs(e.Loop, e.Value));

        InputVariables = [];
        InputVariables.CollectionChanged += InputVariables_CollectionChanged;

        EventHandler invalidateDelegate = (sender, e) => Invalidate();
        GotFocus += invalidateDelegate;
        LostFocus += invalidateDelegate;
    }

    #region IKarnaughMapView
    // Explicit implementations: Control already declares a protected FontHeight, and the
    // view's notion of focus is narrower than Control.Focused's role in the framework.
    int IKarnaughMapView.FontHeight => Font.Height;
    bool IKarnaughMapView.HasFocus => Focused;
    void IKarnaughMapView.ShowError(string message) => MessageBox.Show(message);
    #endregion

    #region Properties
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IQuineMcCluskeySolver? Solver
    {
        get => presenter.Solver;
        set => presenter.Solver = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ObservableCollection.ObservableCollection<string> InputVariables { get; private set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string OutputVariable
    {
        get => presenter.Model.OutputVariable;
        set { presenter.Model.OutputVariable = value; Invalidate(); }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public EEditMode Mode
    {
        get => presenter.Model.Mode;
        set
        {
            var args = new ModeChangingEventArgs(presenter.Model.Mode, value);
            ModeChanging?.Invoke(this, args);
            if (args.Cancel) return;

            presenter.Model.Mode = value;
            if (value == EEditMode.Edit)
            {
                presenter.Model.ClearSolution();
            }
            Invalidate();
        }
    }

    public bool HasLoops => presenter.Model.LoopsOnes.Count != 0 & presenter.Model.LoopsZeros.Count != 0;

    private IRequiredLoop? selectedLoop;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IRequiredLoop? SelectedLoop
    {
        get => selectedLoop;
        set
        {
            selectedLoop = value;
            presenter.Model.SelectLoop(value);
            Invalidate();
        }
    }
    #endregion

    #region Events
    public event EventHandler<KarnaughMapSolvedEventArgs>? KarnaughMapSolved;
    public event EventHandler<KarnaughLoopAddedEventArgs>? KarnaughLoopAdded;
    public event ModeChangingEventHandler? ModeChanging;
    #endregion

    #region Public methods
    public Task RandomFill() => presenter.RandomFill();

    public Task SolveSelection() => presenter.SolveSelection();

    public Task SolveAutomatically() => presenter.SolveAutomatically();
    #endregion

    #region Event handlers
    private void InputVariables_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        presenter.Model.SetInputVariables(InputVariables);
        ApplyPreferredSize();
        Invalidate();
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        ApplyPreferredSize();
    }

    /// <summary>
    /// Sizes the control to fit the grid.
    /// </summary>
    /// <remarks>
    /// Called when the grid or the font changes - the only two things that can change the
    /// required size. It used to be done by assigning Width and Height from inside the
    /// paint handler, which can retrigger layout in the middle of a paint pass.
    /// </remarks>
    private void ApplyPreferredSize()
    {
        var (width, height) = presenter.GetPreferredSize(Font.Height);
        if (Width != width || Height != height)
        {
            Size = new Size(width, height);
        }
    }

    private void KarnaughMap_KeyDown(object sender, KeyEventArgs e)
    {
        if (presenter.HandleCommand(ToCommand(e.KeyCode)))
        {
            e.Handled = true;
        }
    }

    /// <summary>Maps a key code onto a map command. The only place WinForms keys are understood.</summary>
    private static EKarnaughCommand ToCommand(Keys keyCode) => keyCode switch
    {
        Keys.Left => EKarnaughCommand.MoveLeft,
        Keys.Right => EKarnaughCommand.MoveRight,
        Keys.Up => EKarnaughCommand.MoveUp,
        Keys.Down => EKarnaughCommand.MoveDown,
        Keys.Space => EKarnaughCommand.Toggle,
        Keys.D0 or Keys.NumPad0 => EKarnaughCommand.SetZero,
        Keys.D1 or Keys.NumPad1 => EKarnaughCommand.SetOne,
        Keys.X => EKarnaughCommand.SetDontCare,
        Keys.OemMinus => EKarnaughCommand.SetUndefined,
        _ => EKarnaughCommand.None,
    };

    private void KarnaughMap_MouseClick(object sender, MouseEventArgs e)
    {
        presenter.HandleClick(e.X, e.Y, Font.Height);
    }

    protected override bool IsInputKey(Keys keyData) => true;
    #endregion

    #region Painting
    private void KarnaughMap_Paint(object sender, PaintEventArgs e)
    {
        var model = presenter.BuildRenderModel();
        var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        var gridSize = KarnaughMapPresenter.GridSize;

        e.Graphics.Clear(BackColor);
        e.Graphics.DrawString(model.OutputVariable, Font, Brushes.Black, 0, 0);
        e.Graphics.TranslateTransform(Font.Height, Font.Height);

        // Axis captions
        e.Graphics.DrawString(model.ColumnVariables, Font, Brushes.Black,
            new RectangleF(gridSize, -Font.Height, model.ColumnCount * gridSize, Font.Height), format);

        // Grid
        for (var i = 0; i <= model.ColumnCount; i++)
        {
            e.Graphics.DrawLine(Pens.Black, gridSize * (i + 1), gridSize, gridSize * (i + 1), gridSize * (model.RowCount + 1));
        }
        for (var j = 0; j <= model.RowCount; j++)
        {
            e.Graphics.DrawLine(Pens.Black, gridSize, gridSize * (j + 1), gridSize * (model.ColumnCount + 1), gridSize * (j + 1));
        }
        e.Graphics.DrawLine(Pens.Black, 0, 0, gridSize, gridSize);

        // Row labels down the left gutter
        for (var i = 0; i < model.RowLabels.Count; i++)
        {
            e.Graphics.DrawString(model.RowLabels[i], Font, Brushes.Black,
                new RectangleF(0, (i + 1) * gridSize, gridSize, gridSize), format);
        }

        // Column labels along the top gutter, drawn rotated
        var gridTransform = e.Graphics.Transform;
        e.Graphics.RotateTransform(-90);
        e.Graphics.DrawString(model.RowVariables, Font, Brushes.Black,
            new RectangleF(-Height + Font.Height, -Font.Height, Height - Font.Height - gridSize, Font.Height), format);
        for (var i = 0; i < model.ColumnLabels.Count; i++)
        {
            e.Graphics.DrawString(model.ColumnLabels[i], Font, Brushes.Black,
                new RectangleF(-gridSize, (i + 1) * gridSize, gridSize, gridSize), format);
        }
        e.Graphics.Transform = gridTransform;

        // Cell overlays, then cell text
        foreach (var cell in model.Cells)
        {
            var brush = GetFillBrush(cell);
            if (brush != null)
            {
                e.Graphics.FillRectangle(brush,
                    ((cell.Column + 1) * gridSize) + 1, ((cell.Row + 1) * gridSize) + 1, gridSize - 1, gridSize - 1);
            }
        }

        foreach (var cell in model.Cells)
        {
            e.Graphics.DrawString(cell.Text, Font, Brushes.Black,
                new RectangleF((cell.Column + 1) * gridSize, (cell.Row + 1) * gridSize, gridSize, gridSize), format);
        }

        if (model.HasFocus)
        {
            var rect = new Rectangle(
                (model.FocusedColumn + 1) * gridSize, (model.FocusedRow + 1) * gridSize, gridSize, gridSize);
            rect.Inflate(-2, -2);
            using var pen = new Pen(Color.Black, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };
            e.Graphics.DrawRectangle(pen, rect);
        }
    }

    /// <summary>Picks the brush for a cell's overlay. Pure lookup - the choice was already made.</summary>
    private static Brush? GetFillBrush(KarnaughCell cell) => cell.Fill switch
    {
        ECellFill.Selected => Brushes.Yellow,
        ECellFill.LoopOne => cell.Hatched
            ? new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.Percent50, Color.Olive, Color.Transparent)
            : Brushes.Olive,
        ECellFill.LoopZero => cell.Hatched
            ? new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.Percent50, Color.Transparent, Color.OrangeRed)
            : Brushes.OrangeRed,
        _ => null,
    };
    #endregion
}
