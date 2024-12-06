using QuineMcCluskey;

namespace KarnaughMap.Events.EventArgs
{
    public class KarnaughLoopAddedEventArgs : System.EventArgs
    {
        public KarnaughLoopAddedEventArgs(RequiredLoop loop, bool value)
        {
            Loop = loop;
            Value = value;
        }

        public RequiredLoop Loop { get; private set; }
        public bool Value { get; private set; }
    }
}
