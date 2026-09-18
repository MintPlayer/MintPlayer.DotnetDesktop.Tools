using MintPlayer.IconUtils.DllImport;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MintPlayer.IconUtils.Native;

/// <summary>The real kernel32-backed module reader, used by default.</summary>
public sealed class NativeModuleResources : INativeModuleResources
{
    /// <inheritdoc />
    public IModuleHandle Open(string path)
    {
        var handle = Kernel32.LoadLibraryEx(path, IntPtr.Zero, Constants.Kernel32.LOAD_LIBRARY_AS_DATAFILE);
        if (handle == IntPtr.Zero)
        {
            throw new Win32Exception("Failed to load the icon from disk");
        }

        return new NativeModuleHandle(handle);
    }
}

internal sealed class NativeModuleHandle(IntPtr module) : IModuleHandle
{
    public IReadOnlyList<nint> EnumerateIconGroupNames()
    {
        var names = new List<nint>();

        // The callback must stay rooted for the duration of the native call; holding it in
        // a local rather than passing a lambda inline keeps the GC away from it.
        ENUMRESNAMEPROC callback = (_, _, lpName, _) =>
        {
            names.Add(lpName);
            return true;
        };

        Kernel32.EnumResourceNames(module, Constants.Kernel32.RT_GROUP_ICON, callback, IntPtr.Zero);
        GC.KeepAlive(callback);

        return names;
    }

    public byte[] GetIconGroupData(nint name) => GetResourceData(Constants.Kernel32.RT_GROUP_ICON, name);

    public byte[] GetIconImageData(nint id) => GetResourceData(Constants.Kernel32.RT_ICON, id);

    private byte[] GetResourceData(IntPtr type, IntPtr name)
    {
        var hResInfo = Kernel32.FindResource(module, name, type);
        if (hResInfo == IntPtr.Zero)
        {
            throw new Win32Exception();
        }

        var hResData = Kernel32.LoadResource(module, hResInfo);
        if (hResData == IntPtr.Zero)
        {
            throw new Win32Exception();
        }

        var pResData = Kernel32.LockResource(hResData);
        if (pResData == IntPtr.Zero)
        {
            throw new Win32Exception();
        }

        var size = Kernel32.SizeofResource(module, hResInfo);
        if (size == 0)
        {
            throw new Win32Exception();
        }

        var buffer = new byte[size];
        Marshal.Copy(pResData, buffer, 0, buffer.Length);
        return buffer;
    }

    public void Dispose()
    {
        if (module != IntPtr.Zero)
        {
            Kernel32.FreeLibrary(module);
            module = IntPtr.Zero;
        }
    }
}
