namespace MintPlayer.PlatformBrowser.Tests.Fakes;

/// <summary>An in-memory registry key, built up in tests with <see cref="With"/>.</summary>
public sealed class FakeRegistryKey : IRegistryKey
{
    private readonly Dictionary<string, object> values = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, FakeRegistryKey> subKeys = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Sets the key's default (unnamed) value.</summary>
    public FakeRegistryKey WithDefault(object value)
    {
        values[string.Empty] = value;
        return this;
    }

    /// <summary>Sets a named value.</summary>
    public FakeRegistryKey With(string name, object value)
    {
        values[name] = value;
        return this;
    }

    /// <summary>
    /// Adds a child key. The path may contain backslashes, which are created as nested
    /// keys - so a test can write the same paths the production code opens.
    /// </summary>
    public FakeRegistryKey WithSubKey(string path, Action<FakeRegistryKey>? configure = null)
    {
        var current = this;
        foreach (var segment in path.Split('\\', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!current.subKeys.TryGetValue(segment, out var next))
            {
                next = new FakeRegistryKey();
                current.subKeys[segment] = next;
            }
            current = next;
        }

        configure?.Invoke(current);
        return this;
    }

    public string[] GetSubKeyNames() => [.. subKeys.Keys];

    public string[] GetValueNames() => [.. values.Keys.Where(k => k.Length > 0)];

    public object? GetValue(string? name) => values.TryGetValue(name ?? string.Empty, out var value) ? value : null;

    public IRegistryKey? OpenSubKey(string name)
    {
        var current = this;
        foreach (var segment in name.Split('\\', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!current.subKeys.TryGetValue(segment, out var next))
            {
                return null;
            }
            current = next;
        }
        return current;
    }

    public void Dispose()
    {
        // Nothing native to release.
    }
}

/// <summary>An in-memory registry. Paths not explicitly added simply do not exist.</summary>
public sealed class FakeRegistry : IRegistry
{
    /// <summary>Root of HKEY_LOCAL_MACHINE.</summary>
    public FakeRegistryKey LocalMachine { get; } = new();

    /// <summary>Root of HKEY_CURRENT_USER.</summary>
    public FakeRegistryKey CurrentUser { get; } = new();

    /// <summary>Root of HKEY_CLASSES_ROOT.</summary>
    public FakeRegistryKey ClassesRoot { get; } = new();

    public IRegistryKey? OpenLocalMachine(string path) => LocalMachine.OpenSubKey(path);

    public IRegistryKey? OpenCurrentUser(string path) => CurrentUser.OpenSubKey(path);

    public IRegistryKey? OpenClassesRoot(string path) => ClassesRoot.OpenSubKey(path);
}
