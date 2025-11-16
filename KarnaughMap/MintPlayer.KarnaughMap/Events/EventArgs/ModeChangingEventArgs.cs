using MintPlayer.KarnaughMap.Enums;

namespace MintPlayer.KarnaughMap.Events.EventArgs;

public class ModeChangingEventArgs : System.EventArgs
{
    public ModeChangingEventArgs(EEditMode OldValue, EEditMode NewValue)
    {
        this.OldValue = OldValue;
        this.NewValue = NewValue;
    }
    public EEditMode OldValue { get; private set; }
    public EEditMode NewValue { get; private set; }
    public bool Cancel { get; set; }
}
