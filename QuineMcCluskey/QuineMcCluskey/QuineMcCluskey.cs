using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using QuineMcCluskey.Enums;
//using QuineMcCluskey.Data;
using Table1 = QuineMcCluskey.Data.QuineMcCluskey.Table1.Table;
using Table2 = QuineMcCluskey.Data.QuineMcCluskey.Table2.Table;

namespace QuineMcCluskey
{
    public static class QuineMcCluskeySolver
    {
        public static IEnumerable<RequiredLoop> QMC_Solve(IEnumerable<int> minterms, IEnumerable<int> dontcares)
        {
            //var table = QMC_CreateTable(minterms, dontcares);

            // For table 1, include the don't cares
            var table1 = CreateTable1(minterms.Union(dontcares));
            SolveTable1(table1);

            var unused = table1.Columns
                .SelectMany(c => c.Groups)
                .SelectMany(g => g.Records)
                .Where(r => !r.Used);

            var table2 = CreateTable2(minterms.Except(dontcares).ToList(), unused.ToList());

            SolveTable2(table2);

            return table2.Rows.Where(r => r.Status == Data.QuineMcCluskey.Table2.eRowStatus.Required).Select(r => new RequiredLoop(r.Loop));
        }

        private static Table1 CreateTable1(IEnumerable<int> minterms)
        {
            var table = new Table1();

            // If there are no minterms at all, return empty table
            if (!minterms.Any()) return table;

            // Convert to binary
            var bin_minterms = minterms.Select(m => new
            {
                Binary = Convert.ToString(m, 2),
                Decimal = m
            });

            // Compute number of bits
            var bits = bin_minterms.Max(m => m.Binary.Length);

            // Pad the binary numbers, convert to logic states
            var bin_minterms_padded = bin_minterms
                .Select(m => new
                {
                    Binary = m.Binary.PadLeft(bits, '0').Select(b =>
                    {
                        switch (b)
                        {
                            case '0':
                                return LogicState.False;
                            case '1':
                                return LogicState.True;
                            default:
                                return LogicState.DontCare;
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
                table
                    .Columns[0]
                    .Groups[minterm.Binary.Count(n => n == LogicState.True)]
                    .Records.Add(new Data.QuineMcCluskey.Table1.Loop(new[] { minterm.Decimal }, minterm.Binary.ToArray()));

            return table;
        }

        private static void SolveTable1(Table1 table1)
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

                            // Mark records as used
                            term1.Used = term2.Used = true;

                            if (table1.Columns[i + 1].Groups[j].Records.Any(r => res.Data.SequenceEqual(r.Data))) continue;

                            table1.Columns[i + 1].Groups[j].Records.Add(res);
                        }
                    }
                }
            }
        }

        private static Table2 CreateTable2(List<int> minterms, List<Data.QuineMcCluskey.Table1.Loop> loops)
        {
            int loopCount = loops.Count, mintermCount = minterms.Count;

            return new Table2
            {
                Rows = loops.Select(l => new Data.QuineMcCluskey.Table2.Row { Loop = l, Status = Data.QuineMcCluskey.Table2.eRowStatus.Neutral }).ToList(),
                Columns = minterms.Select(m => new Data.QuineMcCluskey.Table2.Column { Minterm = m, Status = Data.QuineMcCluskey.Table2.eColumnStatus.NotUsed }).ToList()
            };
        }

        private static void SolveTable2(Table2 table)
        {
            while (true)
            {
                #region Find required rows. Loop through all columns
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    #region If column already used -> continue
                    if (table.Columns[i].Status == Data.QuineMcCluskey.Table2.eColumnStatus.Used) continue;
                    #endregion

                    #region Check if column only has one row
                    var associated_rows = table.FindRowsForColumn(table.Columns[i]);
                    if (associated_rows.Count() == 1)
                    {
                        var required_row = associated_rows.First();

                        // Mark row as required
                        required_row.Status = Data.QuineMcCluskey.Table2.eRowStatus.Required;

                        // Mark columns for this row as used.
                        foreach (var minterm in required_row.Loop.MinTerms)
                            if (table.Columns.Any(c => c.Minterm == minterm))
                                table.Columns.First(c => c.Minterm == minterm).Status = Data.QuineMcCluskey.Table2.eColumnStatus.Used;
                    }
                    #endregion
                }
                #endregion

                #region If there are no more unused columns -> Break
                if (!table.Columns.Any(c => c.Status == Data.QuineMcCluskey.Table2.eColumnStatus.NotUsed))
                    break;
                #endregion

                #region Try to ignore rows
                var ignored_rows = 0;

                // Combine all rows
                for (int i = 0; i < table.Rows.Count - 1; i++)
                {
                    switch (table.Rows[i].Status)
                    {
                        case Data.QuineMcCluskey.Table2.eRowStatus.Required:
                        case Data.QuineMcCluskey.Table2.eRowStatus.Ignore:
                        case Data.QuineMcCluskey.Table2.eRowStatus.TemporarilyIgnore:
                            continue;
                    }

                    var rowIminterms = table.Rows[i].Loop.MinTerms
                        .Except(table.Columns.Where(c => c.Status == Data.QuineMcCluskey.Table2.eColumnStatus.Used).Select(c => c.Minterm))
                        .ToList();
                    for (int j = i + 1; j < table.Rows.Count; j++)
                    {
                        switch (table.Rows[j].Status)
                        {
                            case Data.QuineMcCluskey.Table2.eRowStatus.Required:
                            case Data.QuineMcCluskey.Table2.eRowStatus.Ignore:
                            case Data.QuineMcCluskey.Table2.eRowStatus.TemporarilyIgnore:
                                continue;
                        }

                        var rowJminterms = table.Rows[j].Loop.MinTerms
                            .Except(table.Columns.Where(c => c.Status == Data.QuineMcCluskey.Table2.eColumnStatus.Used).Select(c => c.Minterm))
                            .ToList();
                        var intersect = rowIminterms.Intersect(rowJminterms).ToArray();

                        var rowIinJ = intersect.Length == rowIminterms.Count;
                        var rowJinI = intersect.Length == rowJminterms.Count;

                        if (rowIinJ && rowJinI)
                        {
                            if (table.Rows[i].Loop.MinTerms.Length > table.Rows[j].Loop.MinTerms.Length)
                                table.Rows[j].Status = Data.QuineMcCluskey.Table2.eRowStatus.Ignore;
                            else
                                table.Rows[i].Status = Data.QuineMcCluskey.Table2.eRowStatus.Ignore;
                            ignored_rows++;
                        }
                        else if (rowIinJ)
                        {
                            table.Rows[i].Status = Data.QuineMcCluskey.Table2.eRowStatus.Ignore;
                            ignored_rows++;
                        }
                        else if (rowJinI)
                        {
                            table.Rows[j].Status = Data.QuineMcCluskey.Table2.eRowStatus.Ignore;
                            ignored_rows++;
                        }
                    }
                }
                #endregion

                #region If we were able to permanently ignore a row -> Rerun
                if (ignored_rows != 0) continue;
                #endregion

                #region Try to temporarily ignore a row -> Rerun
                var neutralRow = table.Rows.FirstOrDefault(c => c.Status == Data.QuineMcCluskey.Table2.eRowStatus.Neutral);
                if (neutralRow == null)
                {
                    // No neutral rows found, however there are still unused minterms
                    var unusedColumn = table.Columns.FirstOrDefault(c => c.Status == Data.QuineMcCluskey.Table2.eColumnStatus.NotUsed);
                    var lastRowForColumn = table.FindRowsForColumn(unusedColumn, true).LastOrDefault(c => c.Status == Data.QuineMcCluskey.Table2.eRowStatus.TemporarilyIgnore);

                    if (lastRowForColumn == null)
                    {
                        // All rows have been ignored, and there are still unused minterms (not supposed to happen)
                        throw new Exception("Unexpected");
                    }

                    lastRowForColumn.Status = Data.QuineMcCluskey.Table2.eRowStatus.Neutral;
                }
                else
                {
                    neutralRow.Status = Data.QuineMcCluskey.Table2.eRowStatus.TemporarilyIgnore;
                }
                #endregion

            }
        }
    }
}
