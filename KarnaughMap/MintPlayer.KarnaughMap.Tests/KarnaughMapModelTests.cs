using MintPlayer.KarnaughMap.Enums;
using MintPlayer.KarnaughMap.Helpers;

namespace MintPlayer.KarnaughMap.Tests;

public class GrayCodeConverterTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 3)]
    [InlineData(3, 2)]
    [InlineData(4, 6)]
    [InlineData(7, 4)]
    public void Converts_a_decimal_to_its_gray_code(int value, int expected)
    {
        Assert.Equal(expected, GrayCodeConverter.Decimal2Gray(value));
    }

    [Theory]
    [InlineData(0), InlineData(1), InlineData(5), InlineData(12), InlineData(255)]
    public void Round_trips(int value)
    {
        Assert.Equal(value, GrayCodeConverter.Gray2Decimal(GrayCodeConverter.Decimal2Gray(value)));
    }

    [Fact]
    public void Adjacent_values_differ_in_exactly_one_bit()
    {
        // This is the property the whole Karnaugh map rests on: neighbouring cells must
        // differ in one variable.
        for (var i = 0; i < 31; i++)
        {
            var difference = GrayCodeConverter.Decimal2Gray(i) ^ GrayCodeConverter.Decimal2Gray(i + 1);
            Assert.Equal(1, System.Numerics.BitOperations.PopCount((uint)difference));
        }
    }
}

public class KarnaughMapModelTests
{
    private static KarnaughMapModel ModelWith(params string[] variables)
    {
        var model = new KarnaughMapModel();
        model.SetInputVariables(variables);
        return model;
    }

    [Fact]
    public void Splits_variables_with_the_first_half_down_the_side()
    {
        var model = ModelWith("A", "B", "C", "D");

        Assert.Equal(["A", "B"], model.RowVariables);
        Assert.Equal(["C", "D"], model.ColumnVariables);
        Assert.Equal(4, model.RowCount);
        Assert.Equal(4, model.ColumnCount);
    }

    [Fact]
    public void Gives_the_odd_variable_to_the_columns()
    {
        var model = ModelWith("A", "B", "C");

        Assert.Equal(["A"], model.RowVariables);
        Assert.Equal(["B", "C"], model.ColumnVariables);
        Assert.Equal(2, model.RowCount);
        Assert.Equal(4, model.ColumnCount);
    }

    [Fact]
    public void Grid_positions_and_minterms_round_trip()
    {
        var model = ModelWith("A", "B", "C", "D");

        for (var column = 0; column < model.ColumnCount; column++)
        {
            for (var row = 0; row < model.RowCount; row++)
            {
                var minterm = model.GridPositionToMinterm(column, row);
                Assert.Equal((column, row), model.MintermToGridPosition(minterm));
            }
        }
    }

    [Fact]
    public void Every_cell_maps_to_a_distinct_minterm()
    {
        var model = ModelWith("A", "B", "C", "D");

        var minterms = from column in Enumerable.Range(0, model.ColumnCount)
                       from row in Enumerable.Range(0, model.RowCount)
                       select model.GridPositionToMinterm(column, row);

        Assert.Equal(16, minterms.Distinct().Count());
    }

    [Fact]
    public void Toggling_walks_the_four_state_cycle()
    {
        var model = ModelWith("A", "B");
        model.Mode = EEditMode.Edit;

        Assert.Equal(ECellValue.Undefined, model.GetValue(0));
        model.ToggleNumber(0);
        Assert.Equal(ECellValue.Zero, model.GetValue(0));
        model.ToggleNumber(0);
        Assert.Equal(ECellValue.One, model.GetValue(0));
        model.ToggleNumber(0);
        Assert.Equal(ECellValue.DontCare, model.GetValue(0));
        model.ToggleNumber(0);
        Assert.Equal(ECellValue.Undefined, model.GetValue(0));
    }

    [Fact]
    public void Toggling_in_solve_mode_selects_instead_of_editing()
    {
        var model = ModelWith("A", "B");
        model.Mode = EEditMode.Solve;

        model.ToggleNumber(2);
        Assert.Equal([2], model.SelectedCells);
        Assert.Equal(ECellValue.Undefined, model.GetValue(2));

        model.ToggleNumber(2);
        Assert.Empty(model.SelectedCells);
    }

    [Theory]
    [InlineData(ECellValue.Zero)]
    [InlineData(ECellValue.One)]
    [InlineData(ECellValue.DontCare)]
    [InlineData(ECellValue.Undefined)]
    public void SetValue_is_idempotent(ECellValue value)
    {
        var model = ModelWith("A", "B");
        model.Mode = EEditMode.Edit;

        model.SetValue(1, value);
        model.SetValue(1, value);

        Assert.Equal(value, model.GetValue(1));
    }

    [Fact]
    public void SetValue_does_nothing_outside_edit_mode()
    {
        var model = ModelWith("A", "B");
        model.Mode = EEditMode.Solve;

        model.SetValue(1, ECellValue.One);

        Assert.Equal(ECellValue.Undefined, model.GetValue(1));
    }

    [Fact]
    public void Focus_wraps_around_both_edges()
    {
        var model = ModelWith("A", "B", "C", "D");

        model.MoveFocus(-1, -1);
        Assert.Equal(3, model.FocusedColumn);
        Assert.Equal(3, model.FocusedRow);

        model.MoveFocus(1, 1);
        Assert.Equal(0, model.FocusedColumn);
        Assert.Equal(0, model.FocusedRow);
    }

    [Fact]
    public void Focus_is_pulled_back_when_the_grid_shrinks()
    {
        var model = ModelWith("A", "B", "C", "D");
        model.SetFocus(3, 3);

        model.SetInputVariables(["A", "B"]);

        Assert.True(model.FocusedColumn < model.ColumnCount);
        Assert.True(model.FocusedRow < model.RowCount);
    }

    [Fact]
    public void SetFocus_ignores_a_position_off_the_grid()
    {
        var model = ModelWith("A", "B");
        model.SetFocus(0, 0);

        model.SetFocus(99, 99);

        Assert.Equal(0, model.FocusedColumn);
        Assert.Equal(0, model.FocusedRow);
    }

    [Fact]
    public void Dont_cares_are_the_cells_holding_both_values()
    {
        var model = ModelWith("A", "B");
        model.Mode = EEditMode.Edit;
        model.SetValue(0, ECellValue.DontCare);
        model.SetValue(1, ECellValue.One);

        Assert.Equal([0], model.DontCares);
    }

    [Fact]
    public void Selecting_a_loop_replaces_the_selection()
    {
        var model = ModelWith("A", "B");
        model.Mode = EEditMode.Solve;
        model.ToggleNumber(3);

        model.SelectLoop(new StubLoop([0, 1]));

        Assert.Equal([0, 1], model.SelectedCells);
    }

    [Fact]
    public void Selecting_a_null_loop_clears_the_selection()
    {
        var model = ModelWith("A", "B");
        model.Mode = EEditMode.Solve;
        model.ToggleNumber(3);

        model.SelectLoop(null);

        Assert.Empty(model.SelectedCells);
    }
}
