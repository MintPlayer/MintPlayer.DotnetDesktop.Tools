using MintPlayer.QuineMcCluskey.Abstractions;
using MintPlayer.QuineMcCluskey.Data.QuineMcCluskey.Table1;
using MintPlayer.QuineMcCluskey.Data.QuineMcCluskey.Table2;
using Table1 = MintPlayer.QuineMcCluskey.Data.QuineMcCluskey.Table1.Table;
using Table2 = MintPlayer.QuineMcCluskey.Data.QuineMcCluskey.Table2.Table;

namespace MintPlayer.QuineMcCluskey;

public class QuineMcCluskeySolver : IQuineMcCluskeySolver
{
    public async Task<IEnumerable<IRequiredLoop>> QMC_Solve(IEnumerable<int> minterms, IEnumerable<int> dontcares)
    {
        return await Task.Run(() =>
        {
            var table1 = CreateTable1(minterms.Union(dontcares));
            SolveTable1(table1);
            var unused = table1.Columns.SelectMany(c => c.Groups).SelectMany(g => g.Records).Where(r => !r.Used);
            var table2 = CreateTable2(minterms.Except(dontcares).ToList(), unused.ToList());
            SolveTable2(table2);
            return table2.Rows
                .Where(r => r.Status == Data.QuineMcCluskey.Table2.eRowStatus.Required)
                .Select(r => new RequiredLoop(r.Loop));
        });
    }

    private Table1 CreateTable1(IEnumerable<int> minterms)
    {
        var table = new Table1();

        if (!minterms.Any()) return table;

        var bin_minterms = minterms.Select(m => new
        {
            Binary = Convert.ToString(m, 2),
            Decimal = m
        });

        var bits = bin_minterms.Max(m => m.Binary.Length);

        var bin_minterms_padded = bin_minterms
            .Select(m => new
            {
                Binary = m.Binary.PadLeft(bits, '0').Select(b =>
                {
                    switch (b)
                    {
                        case '0': return Enums.ELogicState.False;
                        case '1': return Enums.ELogicState.True;
                        default: return Enums.ELogicState.DontCare;
                    }
                }),
                m.Decimal
            });

        for (int i = 0; i <= bits; i++)
        {
            var column = new Data.QuineMcCluskey.Table1.Column();
            for (int j = 0; j < bits - i + 1; j++)
                column.Groups.Add(new Data.QuineMcCluskey.Table1.Group());
            table.Columns.Add(column);
        }

        foreach (var minterm in bin_minterms_padded)
            table.Columns[0].Groups[minterm.Binary.Count(n => n == Enums.ELogicState.True)].Records.Add(new Data.QuineMcCluskey.Table1.Loop(new[] { minterm.Decimal }, minterm.Binary.ToArray()));

        return table;
    }

    private void SolveTable1(Table1 table1)
    {
        for (int i = 0; i < table1.Columns.Count - 1; i++)
        {
            for (int j = 0; j < table1.Columns[i].Groups.Count - 1; j++)
            {
                for (int k = 0; k < table1.Columns[i].Groups[j].Records.Count; k++)
                {
                    var term1 = table1.Columns[i].Groups[j].Records[k];
                    for (int l = 0; l < table1.Columns[i].Groups[j + 1].Records.Count; l++)
                    {
                        var term2 = table1.Columns[i].Groups[j + 1].Records[l];
                        var res = Data.QuineMcCluskey.Table1.Loop.CompareItems(term1, term2);
                        if (res == null) continue;

                        term1.Used = term2.Used = true;

                        if (table1.Columns[i + 1].Groups[j].Records.Any(r => res.Data.SequenceEqual(r.Data))) continue;

                        table1.Columns[i + 1].Groups[j].Records.Add(res);
                    }
                }
            }
        }
    }

    private Table2 CreateTable2(List<int> minterms, List<Data.QuineMcCluskey.Table1.Loop> loops) => new Table2
    {
        Rows = loops.Select(l => new Data.QuineMcCluskey.Table2.Row { Loop = l, Status = Data.QuineMcCluskey.Table2.eRowStatus.Neutral }).ToList(),
        Columns = minterms.Select(m => new Data.QuineMcCluskey.Table2.Column { Minterm = m, Status = Data.QuineMcCluskey.Table2.eColumnStatus.NotUsed }).ToList()
    };

    private void SolveTable2(Table2 table)
    {
        // Step 1: Identify essential prime implicants (unique coverage).
        var uncovered = new HashSet<int>(table.Columns.Select(c => c.Minterm));

        foreach (var column in table.Columns)
        {
            var coveringRows = table.Rows.Where(r => r.Loop.MinTerms.Contains(column.Minterm)).ToList();
            if (coveringRows.Count == 1)
            {
                var row = coveringRows[0];
                if (row.Status != Data.QuineMcCluskey.Table2.eRowStatus.Required)
                {
                    row.Status = Data.QuineMcCluskey.Table2.eRowStatus.Required;
                    foreach (var m in row.Loop.MinTerms) uncovered.Remove(m);
                }
            }
        }

        if (uncovered.Count == 0) return; // All minterms covered by essentials.

        // Step 2: Petrick's Method for remaining uncovered minterms.
        // Represent each prime implicant (row) as a variable.
        // For each uncovered minterm, create a sum (OR) of implicants covering it.
        var sums = new List<List<Data.QuineMcCluskey.Table2.Row>>();
        foreach (var m in uncovered.OrderBy(x => x))
        {
            var rowsCoveringM = table.Rows.Where(r => r.Loop.MinTerms.Contains(m)).ToList();
            // Exclude already required rows (they are taken anyway) but they also cover uncovered minterms.
            if (rowsCoveringM.Any(r => r.Status == Data.QuineMcCluskey.Table2.eRowStatus.Required))
            {
                // If a required row covers this minterm, it's already covered; skip
                continue;
            }
            sums.Add(rowsCoveringM);
        }

        // If after excluding, still uncovered (i.e., sums empty but uncovered not empty), cover with required rows implicitly.
        if (sums.Count == 0)
        {
            // All remaining uncovered were covered by required prime implicants.
            return;
        }

        // Multiply sums to get product (AND) combinations.
        // Start with first sum as individual terms.
        List<HashSet<Data.QuineMcCluskey.Table2.Row>> products = sums[0]
            .Select(r => new HashSet<Data.QuineMcCluskey.Table2.Row> { r })
            .ToList();

        for (int i = 1; i < sums.Count; i++)
        {
            var newProducts = new List<HashSet<Data.QuineMcCluskey.Table2.Row>>();
            foreach (var prod in products)
            {
                foreach (var r in sums[i])
                {
                    var np = new HashSet<Data.QuineMcCluskey.Table2.Row>(prod);
                    np.Add(r);
                    newProducts.Add(np);
                }
            }
            products = SimplifyProductTerms(newProducts);
        }

        // Choose minimal set of implicants among products.
        int minCount = products.Min(p => p.Count);
        var minimalProducts = products.Where(p => p.Count == minCount).ToList();

        // Tie-breaker: minimal literal count.
        int LiteralCount(HashSet<Data.QuineMcCluskey.Table2.Row> set)
        {
            return set.Sum(r => r.Loop.Data.Count(d => d != Enums.ELogicState.DontCare));
        }
        int minLiteralCount = minimalProducts.Min(p => LiteralCount(p));
        var chosen = minimalProducts.First(p => LiteralCount(p) == minLiteralCount);

        // Mark chosen rows as required.
        foreach (var row in chosen)
        {
            if (row.Status != Data.QuineMcCluskey.Table2.eRowStatus.Required)
            {
                row.Status = Data.QuineMcCluskey.Table2.eRowStatus.Required;
            }
        }
    }

    private List<HashSet<Data.QuineMcCluskey.Table2.Row>> SimplifyProductTerms(List<HashSet<Data.QuineMcCluskey.Table2.Row>> terms)
    {
        // Remove supersets: if A ⊆ B then discard B.
        var result = new List<HashSet<Data.QuineMcCluskey.Table2.Row>>();
        foreach (var t in terms.OrderBy(x => x.Count))
        {
            bool isSuperset = result.Any(existing => existing.IsSubsetOf(t));
            if (isSuperset) continue; // t is a superset of an existing smaller/equal term.

            // Remove existing that are supersets of t.
            result.RemoveAll(existing => t.IsSubsetOf(existing));
            result.Add(t);
        }
        return result;
    }
}
