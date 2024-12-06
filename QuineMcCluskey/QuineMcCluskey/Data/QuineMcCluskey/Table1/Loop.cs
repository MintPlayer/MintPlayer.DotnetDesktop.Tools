using System.Linq;
using System.Collections.Generic;
using QuineMcCluskey.Enums;

namespace QuineMcCluskey.Data.QuineMcCluskey.Table1
{
    internal class Loop
    {
        public Loop(int[] minTerms, LogicState[] data)
        {
            MinTerms = minTerms;
            Data = data;
        }

        public LogicState[] Data { get; set; }
        public bool Used { get; set; }
        public int[] MinTerms { get; private set; }

        public override string ToString()
        {
            return new string(Data.Select(d =>
            {
                switch (d)
                {
                    case LogicState.False: return '0';
                    case LogicState.True: return '1';
                    default: return 'X';
                }
            }).ToArray());
        }

        public string ToString(string[] inputVariables)
        {
            return string.Join(" ", inputVariables.Select((v, index) =>
            {
                int t = index - (inputVariables.Length - Data.Length);
                if (t >= 0)
                {
                    switch (Data[t])
                    {
                        case LogicState.False:
                            return $"{v}!";
                        case LogicState.True:
                            return v;
                        default:
                            return string.Empty;
                    }
                }
                else
                {
                    return $"{v}!";
                }
            }).Where(s => !string.IsNullOrEmpty(s)));
        }

        internal static Loop CompareItems(Loop item1, Loop item2)
        {
            var result = new List<LogicState>();
            var differences = 0;
            for (int m = 0; m < item1.Data.Length; m++)
            {
                if ((item1.Data[m] == LogicState.DontCare) ^ (item2.Data[m] == LogicState.DontCare)) return null;

                if (item1.Data[m] == item2.Data[m]) result.Add(item1.Data[m]);
                else if (++differences > 1) return null;
                else result.Add(LogicState.DontCare);
            }

            return new Loop(item1.MinTerms.Union(item2.MinTerms).ToArray(), result.ToArray());
        }
    }
}
