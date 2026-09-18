using Microsoft.Win32;

namespace MintPlayer.PlatformBrowser.Registry;

/// <summary>The real registry, used by default. A thin adapter - it holds no logic.</summary>
public sealed class WindowsRegistry : IRegistry
{
    /// <inheritdoc />
    public IRegistryKey? OpenLocalMachine(string path) => Wrap(Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path));

    /// <inheritdoc />
    public IRegistryKey? OpenCurrentUser(string path) => Wrap(Microsoft.Win32.Registry.CurrentUser.OpenSubKey(path));

    /// <inheritdoc />
    public IRegistryKey? OpenClassesRoot(string path) => Wrap(Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(path));

    private static IRegistryKey? Wrap(RegistryKey? key) => key is null ? null : new WindowsRegistryKey(key);
}

internal sealed class WindowsRegistryKey(RegistryKey key) : IRegistryKey
{
    public string[] GetSubKeyNames() => key.GetSubKeyNames();

    public string[] GetValueNames() => key.GetValueNames();

    public object? GetValue(string? name) => key.GetValue(name);

    public IRegistryKey? OpenSubKey(string name)
    {
        var sub = key.OpenSubKey(name);
        return sub is null ? null : new WindowsRegistryKey(sub);
    }

    public void Dispose() => key.Dispose();
}
