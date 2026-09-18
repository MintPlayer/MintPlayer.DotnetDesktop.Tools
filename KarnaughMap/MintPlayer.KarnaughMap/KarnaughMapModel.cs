using MintPlayer.KarnaughMap.Enums;
using MintPlayer.KarnaughMap.Helpers;
using MintPlayer.QuineMcCluskey.Abstractions;

namespace MintPlayer.KarnaughMap;

/// <summary>
/// The state of a Karnaugh map and the rules that mutate it: which cells hold ones, zeros
/// or both, where the focus is, which cells are selected, and which loops have been found.
/// </summary>
/// <remarks>
/// Extracted from the UserControl. Nothing here touches WinForms, so every rule below is
/// reachable from a test without a message loop - which is the entire point of the split.
/// </remarks>
public sealed class KarnaughMapModel
{
    private readonly List<int> ones = [];
    private readonly List<int> zeros = [];
    private readonly List<int> selectedCells = [];

    /// <summary>Variables mapped onto the horizontal axis.</summary>
    public string[] ColumnVariables { get; private set; } = [];

    /// <summary>Variables mapped onto the vertical axis.</summary>
    public string[] RowVariables { get; private set; } = [];

    /// <summary>Number of columns, always a power of two.</summary>
    public int ColumnCount { get; private set; } = 1;

    /// <summary>Number of rows, always a power of two.</summary>
    public int RowCount { get; private set; } = 1;

    /// <summary>Focused column.</summary>
    public int FocusedColumn { get; private set; }

    /// <summary>Focused row.</summary>
    public int FocusedRow { get; private set; }

    /// <summary>Whether the map is being edited or solved.</summary>
    public EEditMode Mode { get; set; }

    /// <summary>Name shown in the corner of the map.</summary>
    public string OutputVariable { get; set; } = string.Empty;

    /// <summary>Loops covering the ones.</summary>
    public List<IRequiredLoop> LoopsOnes { get; } = [];

    /// <summary>Loops covering the zeros.</summary>
    public List<IRequiredLoop> LoopsZeros { get; } = [];

    /// <summary>Minterms the user has selected for joining.</summary>
    public IReadOnlyList<int> SelectedCells => selectedCells;

    /// <summary>Minterms currently holding a one (a cell holding both is a don't-care).</summary>
    public IReadOnlyList<int> Ones => ones;

    /// <summary>Minterms currently holding a zero.</summary>
    public IReadOnlyList<int> Zeros => zeros;

    /// <summary>
    /// Splits the input variables over the two axes and resizes the grid. The first half
    /// goes down the side, the remainder across the top.
    /// </summary>
    public void SetInputVariables(IEnumerable<string> inputVariables)
    {
        var all = inputVariables as IList<string> ?? [.. inputVariables];

        var rowVariableCount = all.Count >> 1;
        RowVariables = [.. all.Take(rowVariableCount)];
        ColumnVariables = [.. all.Skip(rowVariableCount)];
        RowCount = 1 << rowVariableCount;
        ColumnCount = 1 << (all.Count - rowVariableCount);

        // The grid may have shrunk under the focus.
        FocusedColumn = Math.Min(FocusedColumn, ColumnCount - 1);
        FocusedRow = Math.Min(FocusedRow, RowCount - 1);
    }

    /// <summary>Maps a grid position to the minterm it represents, through the Gray-code ordering.</summary>
    public int GridPositionToMinterm(int column, int row)
    {
        var columnGray = GrayCodeConverter.Decimal2Gray(column);
        var rowGray = GrayCodeConverter.Decimal2Gray(row);
        return (rowGray * (1 << ColumnVariables.Length)) + columnGray;
    }

    /// <summary>Maps a minterm back to its grid position.</summary>
    public (int Column, int Row) MintermToGridPosition(int minterm)
    {
        var rowGray = minterm >> ColumnVariables.Length;
        var columnGray = minterm - (rowGray << ColumnVariables.Length);
        return (GrayCodeConverter.Gray2Decimal(columnGray), GrayCodeConverter.Gray2Decimal(rowGray));
    }

    /// <summary>Reads the logical value of a cell.</summary>
    public ECellValue GetValue(int minterm) => (ones.Contains(minterm), zeros.Contains(minterm)) switch
    {
        (true, true) => ECellValue.DontCare,
        (true, false) => ECellValue.One,
        (false, true) => ECellValue.Zero,
        (false, false) => ECellValue.Undefined,
    };

    /// <summary>
    /// Advances a cell to its next value, or toggles its selection while solving.
    /// </summary>
    /// <remarks>
    /// The edit cycle is Undefined -> Zero -> One -> DontCare -> Undefined, expressed
    /// through membership of the two lists. Three of the four steps happen to be "add to
    /// zeros", which is why this reads shorter than the cycle it implements:
    ///   Undefined (in neither)  + zeros -> Zero
    ///   Zero      (in zeros)    move     -> One
    ///   One       (in ones)     + zeros -> DontCare (in both)
    ///   DontCare  (in both)     clear    -> Undefined
    /// </remarks>
    public void ToggleNumber(int minterm)
    {
        if (Mode != EEditMode.Edit)
        {
            if (!selectedCells.Remove(minterm))
            {
                selectedCells.Add(minterm);
            }
            return;
        }

        if (zeros.Contains(minterm))
        {
            if (ones.Contains(minterm))
            {
                // DontCare -> Undefined
                ones.Remove(minterm);
                zeros.Remove(minterm);
            }
            else
            {
                // Zero -> One
                zeros.Remove(minterm);
                ones.Add(minterm);
            }
        }
        else
        {
            // Undefined -> Zero, or One -> DontCare. Both are "also a zero now".
            zeros.Add(minterm);
        }
    }

    /// <summary>Sets a cell to an explicit value. Ignored outside edit mode.</summary>
    public void SetValue(int minterm, ECellValue value)
    {
        if (Mode != EEditMode.Edit)
        {
            return;
        }

        SetMembership(ones, minterm, value is ECellValue.One or ECellValue.DontCare);
        SetMembership(zeros, minterm, value is ECellValue.Zero or ECellValue.DontCare);
    }

    private static void SetMembership(List<int> list, int minterm, bool shouldContain)
    {
        if (shouldContain)
        {
            if (!list.Contains(minterm))
            {
                list.Add(minterm);
            }
        }
        else
        {
            list.Remove(minterm);
        }
    }

    /// <summary>Moves the focus one cell, wrapping around the edges.</summary>
    public void MoveFocus(int deltaColumn, int deltaRow)
    {
        FocusedColumn = Wrap(FocusedColumn + deltaColumn, ColumnCount);
        FocusedRow = Wrap(FocusedRow + deltaRow, RowCount);
    }

    private static int Wrap(int value, int count) => count <= 0 ? 0 : ((value % count) + count) % count;

    /// <summary>Moves the focus to an absolute position, ignoring positions off the grid.</summary>
    public void SetFocus(int column, int row)
    {
        if (column < 0 || row < 0 || column >= ColumnCount || row >= RowCount)
        {
            return;
        }

        FocusedColumn = column;
        FocusedRow = row;
    }

    /// <summary>The minterm under the focus.</summary>
    public int FocusedMinterm => GridPositionToMinterm(FocusedColumn, FocusedRow);

    /// <summary>Replaces the selection with the cells of a loop, or clears it when null.</summary>
    public void SelectLoop(IRequiredLoop? loop)
    {
        selectedCells.Clear();
        if (loop != null)
        {
            selectedCells.AddRange(loop.MinTerms);
        }
    }

    /// <summary>Clears the current selection.</summary>
    public void ClearSelection() => selectedCells.Clear();

    /// <summary>Drops every loop and selection, as happens when returning to edit mode.</summary>
    public void ClearSolution()
    {
        LoopsOnes.Clear();
        LoopsZeros.Clear();
        selectedCells.Clear();
    }

    /// <summary>Replaces the whole grid with random contents.</summary>
    public void Fill(IEnumerable<int> newOnes, IEnumerable<int> newZeros)
    {
        ones.Clear();
        ones.AddRange(newOnes);
        zeros.Clear();
        zeros.AddRange(newZeros);
    }

    /// <summary>Minterms that are both a one and a zero, i.e. don't-cares.</summary>
    public IEnumerable<int> DontCares => ones.Intersect(zeros);
}
