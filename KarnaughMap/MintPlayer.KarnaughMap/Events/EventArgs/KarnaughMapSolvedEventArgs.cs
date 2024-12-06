using QuineMcCluskey;
using System.Collections.Generic;

namespace KarnaughMap.Events.EventArgs
{
    public class KarnaughMapSolvedEventArgs : System.EventArgs
    {
        public KarnaughMapSolvedEventArgs(List<RequiredLoop> LoopsOnes, List<RequiredLoop> LoopsZeros)
        {
            this.LoopsOnes = LoopsOnes;
            this.LoopsZeros = LoopsZeros;
        }

        public List<RequiredLoop> LoopsOnes { get; private set; }
        public List<RequiredLoop> LoopsZeros { get; private set; }
    }
}
