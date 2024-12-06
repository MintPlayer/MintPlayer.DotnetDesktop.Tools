using System;
using System.Collections.Generic;
using System.Text;

namespace QuineMcCluskey.Data.QuineMcCluskey.Table1
{
    internal class Table
    {
        public Table()
        {
            Columns = new List<Column>();
        }

        public List<Column> Columns { get; set; }
    }
}
