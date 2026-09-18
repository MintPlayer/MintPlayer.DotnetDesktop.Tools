using MintPlayer.QuineMcCluskey.Abstractions;

namespace MintPlayer.QuineMcCluskey.Tests;

/// <summary>
/// Quine-McCluskey has no single correct answer: a truth table can have several equally
/// minimal covers, so asserting on a specific set of loops would encode one arbitrary
/// choice and break on any internal reordering. These tests assert the two properties
/// that hold for EVERY valid minimal cover instead.
/// </summary>
public class QuineMcCluskeySolverTests
{
    public static TheoryData<int[], int[]> TruthTables => new()
    {
        // { minterms, dontcares }
        { [0, 1, 2, 3], [] },               // all of 2 variables -> collapses to one loop
        { [0, 1, 2, 3, 7], [] },
        { [1, 3, 5, 7], [] },               // odd minterms -> a single variable
        { [0, 2, 5, 7], [] },
        { [4, 8, 10, 11, 12, 15], [9, 14] }, // classic textbook case, with don't-cares
        { [0], [] },                         // degenerate: a single minterm
    };

    [Theory]
    [MemberData(nameof(TruthTables))]
    public async Task Solve_covers_every_minterm(int[] minterms, int[] dontcares)
    {
        var solver = new QuineMcCluskeySolver();

        var loops = await solver.QMC_Solve(minterms, dontcares);

        var covered = loops.SelectMany(l => l.MinTerms).ToHashSet();
        Assert.All(minterms, m => Assert.Contains(m, covered));
    }

    [Theory]
    [MemberData(nameof(TruthTables))]
    public async Task Solve_never_covers_a_term_outside_minterms_or_dontcares(int[] minterms, int[] dontcares)
    {
        var solver = new QuineMcCluskeySolver();
        var allowed = minterms.Concat(dontcares).ToHashSet();

        var loops = await solver.QMC_Solve(minterms, dontcares);

        var covered = loops.SelectMany(l => l.MinTerms).Distinct();
        Assert.All(covered, m => Assert.Contains(m, allowed));
    }

    [Fact]
    public async Task Solve_collapses_a_full_two_variable_table_to_a_single_loop()
    {
        var solver = new QuineMcCluskeySolver();

        var loops = await solver.QMC_Solve([0, 1, 2, 3], []);

        // Every combination is true, so the expression reduces to a constant: one loop
        // covering all four minterms.
        var loop = Assert.Single(loops);
        Assert.Equal([0, 1, 2, 3], loop.MinTerms.Order());
    }

    [Fact]
    public async Task Solve_returns_no_loops_for_an_empty_truth_table()
    {
        var solver = new QuineMcCluskeySolver();

        var loops = await solver.QMC_Solve([], []);

        Assert.Empty(loops);
    }

    [Fact]
    public async Task ToString_renders_the_named_input_variables()
    {
        var solver = new QuineMcCluskeySolver();

        var loops = await solver.QMC_Solve([1, 3, 5, 7], []);

        var rendered = string.Join(" + ", loops.Select(l => l.ToString(["A", "B", "C"])));
        Assert.NotEmpty(rendered);
        // Minterms 1,3,5,7 are exactly the odd ones, i.e. C alone; A and B drop out.
        Assert.DoesNotContain("A", rendered);
        Assert.DoesNotContain("B", rendered);
    }
}
