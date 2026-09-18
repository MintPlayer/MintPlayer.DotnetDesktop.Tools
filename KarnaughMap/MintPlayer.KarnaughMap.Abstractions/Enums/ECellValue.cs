namespace MintPlayer.KarnaughMap.Enums;

/// <summary>Defines a cell state for a karnaugh map cell.</summary>
// Widened from internal to public: it moved into the abstractions assembly and is now part
// of the model's surface (KarnaughMapModel.GetValue/SetValue speak in these).
public enum ECellValue
{
    /// <summary>0</summary>
    Zero,
    /// <summary>1</summary>
    One,
    /// <summary>X</summary>
    DontCare,
    /// <summary>-</summary>
    Undefined
}
