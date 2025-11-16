using MintPlayer.QuineMcCluskey.Abstractions;

namespace MintPlayer.KarnaughMap.Events.EventArgs;

public class KarnaughLoopAddedEventArgs : System.EventArgs
{
    public KarnaughLoopAddedEventArgs(IRequiredLoop loop, bool value)
    {
        Loop = loop;
        Value = value;
    }

    public IRequiredLoop Loop { get; private set; }
    public bool Value { get; private set; }
}
