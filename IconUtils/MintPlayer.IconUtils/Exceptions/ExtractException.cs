namespace MintPlayer.IconUtils.Exceptions;

public class ExtractException : Exception
{
    public ExtractException(string message) : base(message)
    {
    }

    public ExtractException(string message, Exception inner) : base(message, inner)
    {
    }
}
