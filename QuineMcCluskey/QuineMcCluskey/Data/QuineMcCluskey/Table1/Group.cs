using System.Collections.Generic;

namespace QuineMcCluskey.Data.QuineMcCluskey.Table1
{
    internal class Group
    {
        public Group()
        {
            Records = new List<Loop>();
        }

        public List<Loop> Records { get; set; }
    }
}
