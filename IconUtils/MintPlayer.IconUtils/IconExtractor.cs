using System.ComponentModel;
using System.Drawing;

namespace MintPlayer.IconUtils;

public static class IconExtractor
{
    /// <summary>Splits up the different images in the icon.</summary>
    /// <param name="filename">Path to the file.</param>
    /// <returns></returns>
    public static async Task<List<Icon>> Split(string filename)
    {
        // Check if filename is provided
        if (string.IsNullOrEmpty(filename))
        {
            throw new ArgumentNullException(nameof(filename));
        }

        // Check if file exists
        if (!File.Exists(filename))
        {
            throw new FileNotFoundException("File not found", filename);
        }

        // Load the icon
        switch (Path.GetExtension(filename))
        {
            case ".exe":
                var resExe = await ExtractIconsFromExe(filename);
                return resExe;
            case ".ico":
            case ".cur":
                var resIco = await Task.Run(async () =>
                {
                    var icon = new Icon(filename);
                    var resIco = await ExtractImagesFromIcon(icon);
                    return resIco;
                });
                return resIco;
            default:
                throw new InvalidOperationException(@"Input file must have one of following extensions: "".exe"", "".ico"", "".cur""");
        }
    }

    public static async Task<List<Icon>> ExtractImagesFromIcon(Icon icon)
    {
        // Check if icon is provided
        if (icon == null)
        {
            throw new ArgumentNullException(nameof(icon));
        }

        var icons = await Utils.IconUtils.Split(icon);
        return icons;
    }

    private static async Task<List<Icon>> ExtractIconsFromExe(string exeFileName)
    {
        var result = await Task.Run(() =>
        {
            // Handle to the icon
            var hIcon = IntPtr.Zero;

            // Try to load the icon
            try
            {
                // Load icon from file
                hIcon = DllImport.Kernel32.LoadLibraryEx(exeFileName, IntPtr.Zero, Constants.Kernel32.LOAD_LIBRARY_AS_DATAFILE);

                if (hIcon == IntPtr.Zero)
                {
                    throw new Win32Exception("Failed to load the icon from disk");
                }

                // Buffer to store the raw data
                var dataBuffer = new List<byte[]>();

                DllImport.ENUMRESNAMEPROC callback = (lpIcon, lpType, lpName, lParam) =>
                {
                    // http://msdn.microsoft.com/en-us/library/ms997538.aspx

                    // RT_GROUP_ICON resource consists of a GRPICONDIR and GRPICONDIRENTRY's.
                    var dir = GetDataFromResource(lpIcon, Constants.Kernel32.RT_GROUP_ICON, lpName);

                    #region Calculate the size of an entire .icon file.
                    // GRPICONDIR.idCount
                    int count = BitConverter.ToUInt16(dir, 4);

                    // sizeof(ICONDIR) + sizeof(ICONDIRENTRY) * count
                    int len = 6 + 16 * count;
                    for (int i = 0; i < count; ++i)
                    {
                        // GRPICONDIRENTRY.dwBytesInRes
                        len += BitConverter.ToInt32(dir, 6 + 14 * i + 8);
                    }
                    #endregion

                    using (var dst = new BinaryWriter(new MemoryStream(len)))
                    {
                        // Copy GRPICONDIR to ICONDIR.
                        dst.Write(dir, 0, 6);

                        // sizeof(ICONDIR) + sizeof(ICONDIRENTRY) * count
                        int picOffset = 6 + 16 * count;

                        for (int i = 0; i < count; ++i)
                        {
                            // Load the picture.

                            // GRPICONDIRENTRY.nID
                            ushort id = BitConverter.ToUInt16(dir, 6 + 14 * i + 12);
                            var pic = GetDataFromResource(hIcon, Constants.Kernel32.RT_ICON, (IntPtr)id);

                            // Copy GRPICONDIRENTRY to ICONDIRENTRY.
                            dst.Seek(6 + 16 * i, SeekOrigin.Begin);
                            // First 8bytes are identical.
                            dst.Write(dir, 6 + 14 * i, 8);
                            // ICONDIRENTRY.dwBytesInRes
                            dst.Write(pic.Length);
                            // ICONDIRENTRY.dwImageOffset
                            dst.Write(picOffset);

                            // Copy a picture.
                            dst.Seek(picOffset, SeekOrigin.Begin);
                            dst.Write(pic, 0, pic.Length);

                            picOffset += pic.Length;
                        }

                        dataBuffer.Add(((MemoryStream)dst.BaseStream).ToArray());
                    }

                    return true;
                };

                DllImport.Kernel32.EnumResourceNames(hIcon, Constants.Kernel32.RT_GROUP_ICON, callback, IntPtr.Zero);

                var result = new List<Icon>();
                for (int i = 0; i < dataBuffer.Count; i++)
                {
                    using (var ms = new MemoryStream(dataBuffer[i]))
                    {
                        result.Add(new Icon(ms));
                    }
                }
                return result;
            }
            finally
            {
                if (hIcon != IntPtr.Zero)
                {
                    DllImport.Kernel32.FreeLibrary(hIcon);
                }
            }
        });
        return result;
    }

    private static byte[] GetDataFromResource(IntPtr hModule, IntPtr type, IntPtr name)
    {
        // Load the binary data from the specified resource.

        IntPtr hResInfo = DllImport.Kernel32.FindResource(hModule, name, type);
        if (hResInfo == IntPtr.Zero)
        {
            throw new Win32Exception();
        }

        IntPtr hResData = DllImport.Kernel32.LoadResource(hModule, hResInfo);
        if (hResData == IntPtr.Zero)
        {
            throw new Win32Exception();
        }

        IntPtr pResData = DllImport.Kernel32.LockResource(hResData);
        if (pResData == IntPtr.Zero)
        {
            throw new Win32Exception();
        }

        uint size = DllImport.Kernel32.SizeofResource(hModule, hResInfo);
        if (size == 0)
        {
            throw new Win32Exception();
        }

        byte[] buf = new byte[size];
        System.Runtime.InteropServices.Marshal.Copy(pResData, buf, 0, buf.Length);

        return buf;
    }
}
