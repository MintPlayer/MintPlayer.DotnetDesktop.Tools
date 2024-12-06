using System.Collections.Generic;

namespace QuineMcCluskey.Data.QuineMcCluskey.Table1
{
    internal class Column
    {
        public Column()
        {
            Groups = new List<Group>();
        }

        public List<Group> Groups { get; set; }
    }
}
