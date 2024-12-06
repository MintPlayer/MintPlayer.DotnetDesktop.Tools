using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuineMcCluskey.Data.QuineMcCluskey.Table2
{
    internal class Table
    {
        public List<Row> Rows { get; set; }
        public List<Column> Columns { get; set; }

        public IEnumerable<Row> FindRowsForColumn(Column column, bool include_temporarily_ignored = false)
        {
            var validRowStatuses = include_temporarily_ignored
                ? new[] { eRowStatus.Neutral, eRowStatus.Required, eRowStatus.TemporarilyIgnore }
                : new[] { eRowStatus.Neutral, eRowStatus.Required };

            return Rows
                .Where(r => validRowStatuses.Contains(r.Status))
                .Where(r => r.Loop.MinTerms.Contains(column.Minterm));
        }
    }
}
