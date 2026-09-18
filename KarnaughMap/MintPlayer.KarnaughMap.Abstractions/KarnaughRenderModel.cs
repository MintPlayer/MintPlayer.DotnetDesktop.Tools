namespace MintPlayer.KarnaughMap;

/// <summary>What kind of overlay a cell carries.</summary>
public enum ECellFill
{
    /// <summary>No overlay.</summary>
    None,
    /// <summary>Covered by a loop over the ones.</summary>
    LoopOne,
    /// <summary>Covered by a loop over the zeros.</summary>
    LoopZero,
    /// <summary>Selected by the user while joining cells.</summary>
    Selected,
}

/// <summary>One cell of the map, resolved to exactly what should be drawn in it.</summary>
/// <param name="Column">Zero-based column.</param>
/// <param name="Row">Zero-based row.</param>
/// <param name="Minterm">The minterm this position maps to, through the Gray-code ordering.</param>
/// <param name="Text">"1", "0", "X" for a don't-care, or "-" when undefined.</param>
/// <param name="Fill">Overlay to paint under the text.</param>
/// <param name="Hatched">
/// Whether the overlay is hatched rather than solid. A loop over the ones that covers a
/// cell which is also a zero is hatched, and vice versa - that is how a don't-care pulled
/// into a loop is distinguished from a plain member.
/// </param>
public sealed record KarnaughCell(
    int Column,
    int Row,
    int Minterm,
    string Text,
    ECellFill Fill,
    bool Hatched);

/// <summary>
/// A complete description of the map, ready to be drawn. Contains no drawing primitives
/// and no Windows types: every decision is already made, so a test can assert on it and
/// the view only walks it.
/// </summary>
/// <param name="ColumnCount">Number of columns.</param>
/// <param name="RowCount">Number of rows.</param>
/// <param name="Cells">Every cell, in column-major order.</param>
/// <param name="ColumnLabels">Gray-coded binary label per column.</param>
/// <param name="RowLabels">Gray-coded binary label per row.</param>
/// <param name="ColumnVariables">Variables mapped onto the horizontal axis, comma separated.</param>
/// <param name="RowVariables">Variables mapped onto the vertical axis, comma separated.</param>
/// <param name="OutputVariable">Name drawn in the top-left corner.</param>
/// <param name="FocusedColumn">Focused column, or -1 when the view does not have focus.</param>
/// <param name="FocusedRow">Focused row, or -1 when the view does not have focus.</param>
/// <param name="PreferredWidth">Width the view should size itself to.</param>
/// <param name="PreferredHeight">Height the view should size itself to.</param>
public sealed record KarnaughRenderModel(
    int ColumnCount,
    int RowCount,
    IReadOnlyList<KarnaughCell> Cells,
    IReadOnlyList<string> ColumnLabels,
    IReadOnlyList<string> RowLabels,
    string ColumnVariables,
    string RowVariables,
    string OutputVariable,
    int FocusedColumn,
    int FocusedRow,
    int PreferredWidth,
    int PreferredHeight)
{
    /// <summary>Whether a focus rectangle should be drawn.</summary>
    public bool HasFocus => FocusedColumn >= 0 && FocusedRow >= 0;
}
