namespace MintPlayer.PlatformBrowser;

/// <summary>
/// The registry seam. Browser detection is mostly parsing - walking a tree of keys and
/// turning their values into <see cref="Browser"/> objects - and only the leaves of that
/// walk actually touch HKLM/HKCU. Putting the walk behind this interface makes the
/// parsing testable without a machine in a particular state.
/// </summary>
public interface IRegistry
{
    /// <summary>Opens a key under HKEY_LOCAL_MACHINE, or returns null when it does not exist.</summary>
    IRegistryKey? OpenLocalMachine(string path);

    /// <summary>Opens a key under HKEY_CURRENT_USER, or returns null when it does not exist.</summary>
    IRegistryKey? OpenCurrentUser(string path);

    /// <summary>Opens a key under HKEY_CLASSES_ROOT, or returns null when it does not exist.</summary>
    IRegistryKey? OpenClassesRoot(string path);
}

/// <summary>A single registry key. Mirrors the subset of RegistryKey this library uses.</summary>
public interface IRegistryKey : IDisposable
{
    /// <summary>Names of the immediate child keys.</summary>
    string[] GetSubKeyNames();

    /// <summary>Names of the values stored directly on this key.</summary>
    string[] GetValueNames();

    /// <summary>Reads a value. Pass null for the key's default (unnamed) value.</summary>
    object? GetValue(string? name);

    /// <summary>Opens a child key, or returns null when it does not exist.</summary>
    IRegistryKey? OpenSubKey(string name);
}
