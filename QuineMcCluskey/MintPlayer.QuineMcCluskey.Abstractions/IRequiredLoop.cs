namespace MintPlayer.QuineMcCluskey.Abstractions;

public interface IRequiredLoop
{
    int[] MinTerms { get; }
    string ToString(string[] inputVariables);
}
