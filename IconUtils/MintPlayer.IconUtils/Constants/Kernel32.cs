namespace MintPlayer.IconUtils.Constants;

internal static class Kernel32
{
    /// <summary>Flag to indicate that the library must be loaded as file</summary>
    internal const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;

    /// <summary>Resource type Icon</summary>
    internal readonly static IntPtr RT_ICON = (IntPtr)3;
    /// <summary>Resource type Group Icon</summary>
    internal readonly static IntPtr RT_GROUP_ICON = (IntPtr)14;

    /// <summary>Maximum path length on the UNIX Operating System</summary>
    internal const int MAX_PATH = 260;
}
