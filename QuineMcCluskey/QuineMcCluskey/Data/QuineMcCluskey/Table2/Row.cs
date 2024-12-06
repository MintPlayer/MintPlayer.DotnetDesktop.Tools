using System;
using System.Collections.Generic;
using System.Text;

namespace QuineMcCluskey.Data.QuineMcCluskey.Table2
{
    internal class Row
    {
        public Table1.Loop Loop { get; set; }
        public eRowStatus Status { get; set; }
    }

    internal enum eRowStatus
    {
        Neutral,
        Required,
        Ignore,
        TemporarilyIgnore
    }
}
