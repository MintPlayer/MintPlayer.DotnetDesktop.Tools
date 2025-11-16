using MintPlayer.QuineMcCluskey.Abstractions;

namespace MintPlayer.KarnaughMap.Events.EventArgs;

public class KarnaughMapSolvedEventArgs : System.EventArgs
{
    public KarnaughMapSolvedEventArgs(List<IRequiredLoop> LoopsOnes, List<IRequiredLoop> LoopsZeros)
    {
        this.LoopsOnes = LoopsOnes;
        this.LoopsZeros = LoopsZeros;
    }

    public List<IRequiredLoop> LoopsOnes { get; private set; }
    public List<IRequiredLoop> LoopsZeros { get; private set; }
}
