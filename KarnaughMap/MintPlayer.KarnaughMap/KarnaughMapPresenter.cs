using MintPlayer.KarnaughMap.Enums;
using MintPlayer.KarnaughMap.Exceptions;
using MintPlayer.KarnaughMap.Helpers;
using MintPlayer.QuineMcCluskey.Abstractions;

namespace MintPlayer.KarnaughMap;

/// <summary>
/// Turns the model into something drawable and translates input into model changes.
/// </summary>
/// <remarks>
/// The presenter never draws. It produces a <see cref="KarnaughRenderModel"/> that already
/// answers every question the paint handler used to ask inline - what text a cell shows,
/// which overlay it carries and whether that overlay is hatched, what the axis labels read,
/// how big the control wants to be - so those decisions can be asserted on directly.
/// </remarks>
public sealed class KarnaughMapPresenter
{
    /// <summary>Side of one cell, in pixels.</summary>
    public const int GridSize = 40;

    private readonly IKarnaughMapView view;

    /// <summary>The state this presenter renders and mutates.</summary>
    public KarnaughMapModel Model { get; }

    /// <summary>Solver used to minimise selections and the whole map.</summary>
    public IQuineMcCluskeySolver? Solver { get; set; }

    /// <summary>Raised when the whole map has been solved.</summary>
    public event EventHandler<(IReadOnlyList<IRequiredLoop> Ones, IReadOnlyList<IRequiredLoop> Zeros)>? Solved;

    /// <summary>Raised when a single loop has been added from a selection.</summary>
    public event EventHandler<(IRequiredLoop Loop, bool Value)>? LoopAdded;

    /// <summary>Creates a presenter over a view, with a fresh model.</summary>
    public KarnaughMapPresenter(IKarnaughMapView view) : this(view, new KarnaughMapModel())
    {
    }

    /// <summary>Creates a presenter over a view and an existing model.</summary>
    public KarnaughMapPresenter(IKarnaughMapView view, KarnaughMapModel model)
    {
        this.view = view;
        Model = model;
    }

    /// <summary>
    /// Size the view should take, given its font. The old paint handler computed this and
    /// assigned Width/Height from inside OnPaint, which can retrigger layout mid-paint.
    /// </summary>
    public (int Width, int Height) GetPreferredSize(int fontHeight) =>
        (((Model.ColumnCount + 1) * GridSize) + 1 + fontHeight,
         ((Model.RowCount + 1) * GridSize) + 1 + fontHeight);

    /// <summary>
    /// Maps a mouse position onto a cell, or null when the click landed in the axis
    /// gutters rather than on the grid.
    /// </summary>
    public (int Column, int Row)? HitTest(int x, int y, int fontHeight)
    {
        var origin = GridSize + fontHeight;
        if (x < origin || y < origin)
        {
            return null;
        }

        var column = (x - origin) / GridSize;
        var row = (y - origin) / GridSize;

        if (column >= Model.ColumnCount || row >= Model.RowCount)
        {
            return null;
        }

        return (column, row);
    }

    /// <summary>Applies a click. Returns whether anything changed.</summary>
    public bool HandleClick(int x, int y, int fontHeight)
    {
        if (HitTest(x, y, fontHeight) is not var (column, row))
        {
            return false;
        }

        Model.SetFocus(column, row);
        Model.ToggleNumber(Model.GridPositionToMinterm(column, row));
        view.Invalidate();
        return true;
    }

    /// <summary>Applies a keyboard command. Returns whether the command was handled.</summary>
    public bool HandleCommand(EKarnaughCommand command)
    {
        switch (command)
        {
            case EKarnaughCommand.MoveLeft: Model.MoveFocus(-1, 0); break;
            case EKarnaughCommand.MoveRight: Model.MoveFocus(1, 0); break;
            case EKarnaughCommand.MoveUp: Model.MoveFocus(0, -1); break;
            case EKarnaughCommand.MoveDown: Model.MoveFocus(0, 1); break;
            case EKarnaughCommand.Toggle: Model.ToggleNumber(Model.FocusedMinterm); break;
            case EKarnaughCommand.SetZero: Model.SetValue(Model.FocusedMinterm, ECellValue.Zero); break;
            case EKarnaughCommand.SetOne: Model.SetValue(Model.FocusedMinterm, ECellValue.One); break;
            case EKarnaughCommand.SetDontCare: Model.SetValue(Model.FocusedMinterm, ECellValue.DontCare); break;
            case EKarnaughCommand.SetUndefined: Model.SetValue(Model.FocusedMinterm, ECellValue.Undefined); break;
            default: return false;
        }

        view.Invalidate();
        return true;
    }

    /// <summary>Fills the map with random contents. Ignored outside edit mode.</summary>
    public async Task RandomFill()
    {
        if (Model.Mode != EEditMode.Edit)
        {
            return;
        }

        var max = 1 << (Model.ColumnVariables.Length + Model.RowVariables.Length);
        Model.Fill(await RandomMinterms(max), await RandomMinterms(max));
        view.Invalidate();
    }

    private static async Task<List<int>> RandomMinterms(int max)
    {
        return await Task.Run(() =>
        {
            var random = new Random();
            var list = new List<int>();
            for (var i = 0; i < max; i++)
            {
                var num = random.Next(max);
                if (!list.Contains(num))
                {
                    list.Add(num);
                }
            }
            return list;
        }).ConfigureAwait(false);
    }

    /// <summary>Minimises the current selection into a single loop.</summary>
    public async Task SolveSelection()
    {
        try
        {
            if (Model.Mode != EEditMode.Solve)
            {
                return;
            }

            if (Solver == null)
            {
                throw new InvalidOperationException("Solver not set for KarnaughMap.");
            }

            if (Model.SelectedCells.Count == 0)
            {
                throw new MinificationException("Please select some cells to join.");
            }

            var selectedOnes = Model.Ones.Except(Model.Zeros).Intersect(Model.SelectedCells).ToList();
            var selectedZeros = Model.Zeros.Except(Model.Ones).Intersect(Model.SelectedCells).ToList();
            var selectedDontCares = Model.DontCares.Intersect(Model.SelectedCells).ToList();

            bool value;
            if (selectedOnes.Count != 0)
            {
                if (selectedZeros.Count != 0)
                {
                    throw new MinificationException("Selected minterms must have the same value.");
                }
                value = true;
            }
            else if (selectedZeros.Count != 0)
            {
                value = false;
            }
            else
            {
                throw new MinificationException("Selected minterms cannot all be don't cares.");
            }

            var result = (await Solver.QMC_Solve(value ? selectedOnes : selectedZeros, selectedDontCares)).ToList();
            if (result.Count != 1)
            {
                throw new MinificationException("Selected minterms cannot be simplified.");
            }

            (value ? Model.LoopsOnes : Model.LoopsZeros).Add(result[0]);
            Model.ClearSelection();
            LoopAdded?.Invoke(this, (result[0], value));
        }
        catch (Exception ex)
        {
            // Goes through the view rather than MessageBox.Show, so a test sees the message
            // instead of blocking on a modal dialog.
            view.ShowError(ex.Message);
        }
        finally
        {
            view.Invalidate();
        }
    }

    /// <summary>Minimises the whole map.</summary>
    public async Task SolveAutomatically()
    {
        try
        {
            if (Model.Mode != EEditMode.Solve)
            {
                return;
            }

            if (Solver == null)
            {
                throw new InvalidOperationException("Solver not set for KarnaughMap.");
            }

            var dontCares = Model.DontCares.ToList();
            var solvedOnes = (await Solver.QMC_Solve(Model.Ones, dontCares)).ToList();
            var solvedZeros = (await Solver.QMC_Solve(Model.Zeros, dontCares)).ToList();

            Model.LoopsOnes.Clear();
            Model.LoopsOnes.AddRange(solvedOnes);
            Model.LoopsZeros.Clear();
            Model.LoopsZeros.AddRange(solvedZeros);

            Solved?.Invoke(this, (solvedOnes, solvedZeros));
        }
        catch (Exception ex)
        {
            view.ShowError(ex.Message);
        }
        finally
        {
            view.Invalidate();
        }
    }

    /// <summary>Builds a complete description of what the view should draw.</summary>
    public KarnaughRenderModel BuildRenderModel()
    {
        var loopOneMinterms = Model.LoopsOnes.SelectMany(l => l.MinTerms).ToHashSet();
        var loopZeroMinterms = Model.LoopsZeros.SelectMany(l => l.MinTerms).ToHashSet();
        var selected = Model.Mode == EEditMode.Solve ? Model.SelectedCells.ToHashSet() : [];

        var cells = new List<KarnaughCell>(Model.ColumnCount * Model.RowCount);
        for (var column = 0; column < Model.ColumnCount; column++)
        {
            for (var row = 0; row < Model.RowCount; row++)
            {
                var minterm = Model.GridPositionToMinterm(column, row);
                cells.Add(new KarnaughCell(
                    column,
                    row,
                    minterm,
                    CellText(Model.GetValue(minterm)),
                    ResolveFill(minterm, loopOneMinterms, loopZeroMinterms, selected, out var hatched),
                    hatched));
            }
        }

        var (width, height) = GetPreferredSize(view.FontHeight);

        return new KarnaughRenderModel(
            Model.ColumnCount,
            Model.RowCount,
            cells,
            BuildAxisLabels(Model.ColumnCount, Model.ColumnVariables.Length),
            BuildAxisLabels(Model.RowCount, Model.RowVariables.Length),
            string.Join(", ", Model.ColumnVariables),
            string.Join(", ", Model.RowVariables),
            Model.OutputVariable ?? string.Empty,
            view.HasFocus ? Model.FocusedColumn : -1,
            view.HasFocus ? Model.FocusedRow : -1,
            width,
            height);
    }

    private static string CellText(ECellValue value) => value switch
    {
        ECellValue.DontCare => "X",
        ECellValue.One => "1",
        ECellValue.Zero => "0",
        _ => "-",
    };

    /// <summary>
    /// Resolves the single overlay a cell ends up with. The old paint handler drew the
    /// ones-loops, then the zeros-loops, then the selection on top, so later wins:
    /// Selected beats LoopZero beats LoopOne.
    /// </summary>
    private ECellFill ResolveFill(
        int minterm,
        HashSet<int> loopOnes,
        HashSet<int> loopZeros,
        HashSet<int> selected,
        out bool hatched)
    {
        hatched = false;

        if (selected.Contains(minterm))
        {
            return ECellFill.Selected;
        }

        if (loopZeros.Contains(minterm))
        {
            // A zeros-loop covering a cell that is also a one is covering a don't-care.
            hatched = Model.Ones.Contains(minterm);
            return ECellFill.LoopZero;
        }

        if (loopOnes.Contains(minterm))
        {
            hatched = Model.Zeros.Contains(minterm);
            return ECellFill.LoopOne;
        }

        return ECellFill.None;
    }

    /// <summary>Gray-coded binary label for each position along an axis.</summary>
    private static string[] BuildAxisLabels(int count, int variableCount)
    {
        var labels = new string[count];
        for (var i = 0; i < count; i++)
        {
            labels[i] = Convert.ToString(GrayCodeConverter.Decimal2Gray(i), 2).PadLeft(variableCount, '0');
        }
        return labels;
    }
}
