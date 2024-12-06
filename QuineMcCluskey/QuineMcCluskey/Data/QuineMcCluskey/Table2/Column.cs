using System;
using System.Collections.Generic;
using System.Text;

namespace QuineMcCluskey.Data.QuineMcCluskey.Table2
{
    internal class Column
    {
        public int Minterm { get; set; }
        public eColumnStatus Status { get; set; }

        public override string ToString()
        {
            return Minterm.ToString() + (Status == eColumnStatus.Used ? "*" : "");
        }
    }

    internal enum eColumnStatus
    {
        NotUsed,
        Used,
    }
}
