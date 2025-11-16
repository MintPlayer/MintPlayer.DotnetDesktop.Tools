using MintPlayer.QuineMcCluskey.Abstractions;

namespace MintPlayer.QuineMcCluskey;

internal class RequiredLoop : IRequiredLoop
{
    internal RequiredLoop(Data.QuineMcCluskey.Table1.Loop loop)
    {
        MinTerms = loop.MinTerms;
        this.loop = loop;
    }

    private Data.QuineMcCluskey.Table1.Loop loop;
    public int[] MinTerms { get; private set; }
    public override string ToString() => loop.ToString();
    public string ToString(string[] inputVariables) => loop.ToString(inputVariables);
}
