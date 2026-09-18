using MintPlayer.KarnaughMap.Enums;
using MintPlayer.QuineMcCluskey;
using MintPlayer.QuineMcCluskey.Abstractions;

namespace MintPlayer.KarnaughMap.Tests;

/// <summary>A loop over a fixed set of minterms.</summary>
public sealed class StubLoop(int[] minTerms) : IRequiredLoop
{
    public int[] MinTerms { get; } = minTerms;

    public string ToString(string[] inputVariables) => string.Join("", MinTerms);
}

public class KarnaughMapPresenterTests
{
    private static (KarnaughMapPresenter Presenter, FakeKarnaughMapView View) Create(params string[] variables)
    {
        var view = new FakeKarnaughMapView();
        var presenter = new KarnaughMapPresenter(view) { Solver = new QuineMcCluskeySolver() };
        presenter.Model.SetInputVariables(variables.Length == 0 ? ["A", "B", "C", "D"] : variables);
        return (presenter, view);
    }

    #region Render model

    [Fact]
    public void Renders_one_cell_per_grid_position()
    {
        var (presenter, _) = Create();

        var model = presenter.BuildRenderModel();

        Assert.Equal(16, model.Cells.Count);
        Assert.Equal(16, model.Cells.Select(c => (c.Column, c.Row)).Distinct().Count());
    }

    [Theory]
    [InlineData(ECellValue.Undefined, "-")]
    [InlineData(ECellValue.Zero, "0")]
    [InlineData(ECellValue.One, "1")]
    [InlineData(ECellValue.DontCare, "X")]
    public void Renders_the_cell_text_for_each_value(ECellValue value, string expected)
    {
        var (presenter, _) = Create();
        presenter.Model.Mode = EEditMode.Edit;
        presenter.Model.SetValue(presenter.Model.GridPositionToMinterm(0, 0), value);

        var cell = presenter.BuildRenderModel().Cells.Single(c => c is { Column: 0, Row: 0 });

        Assert.Equal(expected, cell.Text);
    }

    [Fact]
    public void Renders_axis_labels_as_gray_coded_binary()
    {
        var (presenter, _) = Create("A", "B", "C", "D");

        var model = presenter.BuildRenderModel();

        // Two variables per axis, so two-bit labels in Gray order.
        Assert.Equal(["00", "01", "11", "10"], model.ColumnLabels);
        Assert.Equal(["00", "01", "11", "10"], model.RowLabels);
    }

    [Fact]
    public void Renders_the_axis_captions()
    {
        var (presenter, _) = Create("A", "B", "C", "D");

        var model = presenter.BuildRenderModel();

        Assert.Equal("A, B", model.RowVariables);
        Assert.Equal("C, D", model.ColumnVariables);
    }

    [Fact]
    public void A_cell_in_a_ones_loop_is_filled_solid()
    {
        var (presenter, _) = Create();
        presenter.Model.Mode = EEditMode.Edit;
        presenter.Model.SetValue(0, ECellValue.One);
        presenter.Model.LoopsOnes.Add(new StubLoop([0]));

        var cell = presenter.BuildRenderModel().Cells.Single(c => c.Minterm == 0);

        Assert.Equal(ECellFill.LoopOne, cell.Fill);
        Assert.False(cell.Hatched);
    }

    [Fact]
    public void A_dont_care_pulled_into_a_ones_loop_is_hatched()
    {
        var (presenter, _) = Create();
        presenter.Model.Mode = EEditMode.Edit;
        presenter.Model.SetValue(0, ECellValue.DontCare);
        presenter.Model.LoopsOnes.Add(new StubLoop([0]));

        var cell = presenter.BuildRenderModel().Cells.Single(c => c.Minterm == 0);

        Assert.Equal(ECellFill.LoopOne, cell.Fill);
        Assert.True(cell.Hatched);
    }

    [Fact]
    public void A_dont_care_pulled_into_a_zeros_loop_is_hatched()
    {
        var (presenter, _) = Create();
        presenter.Model.Mode = EEditMode.Edit;
        presenter.Model.SetValue(0, ECellValue.DontCare);
        presenter.Model.LoopsZeros.Add(new StubLoop([0]));

        var cell = presenter.BuildRenderModel().Cells.Single(c => c.Minterm == 0);

        Assert.Equal(ECellFill.LoopZero, cell.Fill);
        Assert.True(cell.Hatched);
    }

    [Fact]
    public void A_zeros_loop_paints_over_a_ones_loop()
    {
        // The old paint handler drew ones, then zeros, then the selection, so later won.
        var (presenter, _) = Create();
        presenter.Model.LoopsOnes.Add(new StubLoop([0]));
        presenter.Model.LoopsZeros.Add(new StubLoop([0]));

        var cell = presenter.BuildRenderModel().Cells.Single(c => c.Minterm == 0);

        Assert.Equal(ECellFill.LoopZero, cell.Fill);
    }

    [Fact]
    public void The_selection_paints_over_every_loop()
    {
        var (presenter, _) = Create();
        presenter.Model.Mode = EEditMode.Solve;
        presenter.Model.LoopsOnes.Add(new StubLoop([0]));
        presenter.Model.LoopsZeros.Add(new StubLoop([0]));
        presenter.Model.ToggleNumber(0);

        var cell = presenter.BuildRenderModel().Cells.Single(c => c.Minterm == 0);

        Assert.Equal(ECellFill.Selected, cell.Fill);
    }

    [Fact]
    public void The_selection_is_not_painted_while_editing()
    {
        var (presenter, _) = Create();
        presenter.Model.Mode = EEditMode.Solve;
        presenter.Model.ToggleNumber(0);
        presenter.Model.Mode = EEditMode.Edit;

        var cell = presenter.BuildRenderModel().Cells.Single(c => c.Minterm == 0);

        Assert.Equal(ECellFill.None, cell.Fill);
    }

    [Fact]
    public void Reports_no_focused_cell_when_the_view_is_not_focused()
    {
        var (presenter, view) = Create();
        view.HasFocus = false;

        var model = presenter.BuildRenderModel();

        Assert.False(model.HasFocus);
        Assert.Equal(-1, model.FocusedColumn);
    }

    [Fact]
    public void Preferred_size_covers_the_grid_plus_the_axis_gutters()
    {
        var (presenter, _) = Create("A", "B", "C", "D");

        var (width, height) = presenter.GetPreferredSize(fontHeight: 15);

        // (4 + 1) cells of 40px, plus a 1px edge, plus one line of font for the caption.
        Assert.Equal((5 * 40) + 1 + 15, width);
        Assert.Equal((5 * 40) + 1 + 15, height);
    }

    #endregion

    #region Input

    [Fact]
    public void A_click_on_the_grid_focuses_and_toggles_that_cell()
    {
        var (presenter, view) = Create();
        presenter.Model.Mode = EEditMode.Edit;
        var origin = KarnaughMapPresenter.GridSize + view.FontHeight;

        var handled = presenter.HandleClick(origin + 5, origin + 45, view.FontHeight);

        Assert.True(handled);
        Assert.Equal(0, presenter.Model.FocusedColumn);
        Assert.Equal(1, presenter.Model.FocusedRow);
        Assert.Equal(ECellValue.Zero, presenter.Model.GetValue(presenter.Model.GridPositionToMinterm(0, 1)));
    }

    [Fact]
    public void A_click_in_the_axis_gutter_is_ignored()
    {
        var (presenter, view) = Create();

        Assert.False(presenter.HandleClick(1, 1, view.FontHeight));
        Assert.Null(presenter.HitTest(1, 1, view.FontHeight));
    }

    [Fact]
    public void A_click_past_the_last_cell_is_ignored()
    {
        var (presenter, view) = Create();
        var origin = KarnaughMapPresenter.GridSize + view.FontHeight;

        Assert.Null(presenter.HitTest(origin + (40 * 99), origin, view.FontHeight));
    }

    [Theory]
    [InlineData(EKarnaughCommand.MoveRight, 1, 0)]
    [InlineData(EKarnaughCommand.MoveDown, 0, 1)]
    [InlineData(EKarnaughCommand.MoveLeft, 3, 0)]
    [InlineData(EKarnaughCommand.MoveUp, 0, 3)]
    public void Movement_commands_move_the_focus(EKarnaughCommand command, int expectedColumn, int expectedRow)
    {
        var (presenter, _) = Create();

        Assert.True(presenter.HandleCommand(command));
        Assert.Equal(expectedColumn, presenter.Model.FocusedColumn);
        Assert.Equal(expectedRow, presenter.Model.FocusedRow);
    }

    [Theory]
    [InlineData(EKarnaughCommand.SetZero, ECellValue.Zero)]
    [InlineData(EKarnaughCommand.SetOne, ECellValue.One)]
    [InlineData(EKarnaughCommand.SetDontCare, ECellValue.DontCare)]
    public void Value_commands_set_the_focused_cell(EKarnaughCommand command, ECellValue expected)
    {
        var (presenter, _) = Create();
        presenter.Model.Mode = EEditMode.Edit;

        Assert.True(presenter.HandleCommand(command));
        Assert.Equal(expected, presenter.Model.GetValue(presenter.Model.FocusedMinterm));
    }

    [Fact]
    public void An_unhandled_command_is_reported_as_unhandled_and_repaints_nothing()
    {
        var (presenter, view) = Create();

        Assert.False(presenter.HandleCommand(EKarnaughCommand.None));
        Assert.Equal(0, view.InvalidateCount);
    }

    #endregion

    #region Solving

    [Fact]
    public async Task Solving_a_selection_adds_one_loop_and_clears_the_selection()
    {
        var (presenter, view) = Create("A", "B");
        presenter.Model.Mode = EEditMode.Edit;
        presenter.Model.SetValue(0, ECellValue.One);
        presenter.Model.SetValue(1, ECellValue.One);
        presenter.Model.Mode = EEditMode.Solve;
        presenter.Model.ToggleNumber(0);
        presenter.Model.ToggleNumber(1);

        var raised = 0;
        presenter.LoopAdded += (_, _) => raised++;

        await presenter.SolveSelection();

        Assert.Empty(view.Errors);
        Assert.Single(presenter.Model.LoopsOnes);
        Assert.Empty(presenter.Model.SelectedCells);
        Assert.Equal(1, raised);
    }

    [Fact]
    public async Task Solving_an_empty_selection_reports_an_error_through_the_view()
    {
        var (presenter, view) = Create("A", "B");
        presenter.Model.Mode = EEditMode.Solve;

        await presenter.SolveSelection();

        Assert.Contains("Please select some cells to join.", view.Errors);
    }

    [Fact]
    public async Task Solving_a_mixed_selection_reports_an_error()
    {
        var (presenter, view) = Create("A", "B");
        presenter.Model.Mode = EEditMode.Edit;
        presenter.Model.SetValue(0, ECellValue.One);
        presenter.Model.SetValue(1, ECellValue.Zero);
        presenter.Model.Mode = EEditMode.Solve;
        presenter.Model.ToggleNumber(0);
        presenter.Model.ToggleNumber(1);

        await presenter.SolveSelection();

        Assert.Contains("Selected minterms must have the same value.", view.Errors);
    }

    [Fact]
    public async Task Solving_without_a_solver_reports_an_error_instead_of_throwing()
    {
        var view = new FakeKarnaughMapView();
        var presenter = new KarnaughMapPresenter(view);
        presenter.Model.SetInputVariables(["A", "B"]);
        presenter.Model.Mode = EEditMode.Solve;

        await presenter.SolveAutomatically();

        Assert.Contains("Solver not set for KarnaughMap.", view.Errors);
    }

    [Fact]
    public async Task Solving_automatically_replaces_the_previous_loops()
    {
        var (presenter, _) = Create("A", "B");
        presenter.Model.Mode = EEditMode.Edit;
        presenter.Model.SetValue(0, ECellValue.One);
        presenter.Model.SetValue(1, ECellValue.One);
        presenter.Model.Mode = EEditMode.Solve;
        presenter.Model.LoopsOnes.Add(new StubLoop([99]));

        await presenter.SolveAutomatically();

        Assert.DoesNotContain(presenter.Model.LoopsOnes, l => l.MinTerms.Contains(99));
    }

    [Fact]
    public async Task Solving_is_ignored_while_editing()
    {
        var (presenter, view) = Create("A", "B");
        presenter.Model.Mode = EEditMode.Edit;

        await presenter.SolveAutomatically();
        await presenter.SolveSelection();

        Assert.Empty(view.Errors);
        Assert.Empty(presenter.Model.LoopsOnes);
    }

    [Fact]
    public async Task Random_fill_only_fills_while_editing()
    {
        var (presenter, _) = Create("A", "B");
        presenter.Model.Mode = EEditMode.Solve;

        await presenter.RandomFill();

        Assert.Empty(presenter.Model.Ones);
        Assert.Empty(presenter.Model.Zeros);
    }

    #endregion
}
