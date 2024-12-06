using KarnaughMap.Enums;

namespace KarnaughMap.Events.EventArgs
{
    public class ModeChangingEventArgs : System.EventArgs
    {
        public ModeChangingEventArgs(eEditMode OldValue, eEditMode NewValue)
        {
            this.OldValue = OldValue;
            this.NewValue = NewValue;
        }
        public eEditMode OldValue { get; private set; }
        public eEditMode NewValue { get; private set; }
        public bool Cancel { get; set; }
    }
}
