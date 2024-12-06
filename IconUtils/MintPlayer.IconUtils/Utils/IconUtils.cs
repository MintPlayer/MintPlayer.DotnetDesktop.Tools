using MintPlayer.IconUtils.Exceptions;
using System.Drawing;
using System.Reflection;
using System.Reflection.Emit;

namespace MintPlayer.IconUtils.Utils;

internal static class IconUtils
{
    static IconUtils()
    {
        try
        {
            // Create dynamic method to access the Icon.iconData private field
            var dm = new DynamicMethod("GetIconData", typeof(byte[]), new[] { typeof(Icon) }, typeof(Icon));

            // Reference to the Icon._iconData private field
            FieldInfo fi;
            var fieldNames = typeof(Icon).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Select(f => f.Name);
            if (fieldNames.Contains("iconData"))
            {
                // .NET Framework
                fi = typeof(Icon).GetField("iconData", BindingFlags.Instance | BindingFlags.NonPublic)!;
            }
            else if (fieldNames.Contains("_iconData"))
            {
                // .NET Core
                fi = typeof(Icon).GetField("_iconData", BindingFlags.Instance | BindingFlags.NonPublic)!;
            }
            else
            {
                throw new InvalidOperationException("IconData field does not exist");
            }

            // Emit commands to 
            var gen = dm.GetILGenerator();
            // - Load the icon on the evaluation stack
            gen.Emit(OpCodes.Ldarg_0);
            // - Find the value of the icon.iconData field
            gen.Emit(OpCodes.Ldfld, fi);
            // - Return the byte array
            gen.Emit(OpCodes.Ret);

            getIconData = (GetIconDataDelegate)dm.CreateDelegate(typeof(GetIconDataDelegate));

        }
        catch (Exception ex)
        {
            throw new ExtractException("Could not initialize IconUtils", ex);
        }
    }

    private delegate byte[] GetIconDataDelegate(Icon icon);
    private static GetIconDataDelegate getIconData;
    private static async Task<byte[]> GetIconData(Icon icon)
    {
        var result = await Task.Run(() =>
        {
            var data = getIconData(icon);
            if (data != null)
            {
                return data;
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    icon.Save(ms);
                    return ms.ToArray();
                }
            }
        });
        return result;
    }


    /// <summary>
    /// Split an Icon consists of multiple icons into an array of Icon each
    /// consists of single icons.
    /// </summary>
    /// <param name="icon">A System.Drawing.Icon to be split.</param>
    /// <returns>An array of System.Drawing.Icon.</returns>
    internal static async Task<List<Icon>> Split(Icon icon)
    {
        if (icon == null)
        {
            throw new ArgumentNullException("icon");
        }

        // Get an .ico file in memory, then split it into separate icons.
        var src = await GetIconData(icon);

        var result = await Task.Run(() =>
        {
            var splitIcons = new List<Icon>();
            // ICONDIR.wImageCount
            int count = BitConverter.ToUInt16(src, 4);

            for (int i = 0; i < count; i++)
            {
                // ICONDIR.dwBytesInRes
                int length = BitConverter.ToInt32(src, 6 + 16 * i + 8);
                // ICONDIR.dwImageOffset
                int offset = BitConverter.ToInt32(src, 6 + 16 * i + 12);

                using (var dst = new BinaryWriter(new MemoryStream(6 + 16 + length)))
                {
                    // Copy ICONDIR and set idCount to 1.
                    dst.Write(src, 0, 4);
                    dst.Write((short)1);

                    // Copy ICONDIRENTRY and set dwImageOffset to 22.
                    dst.Write(src, 6 + 16 * i, 12); // ICONDIRENTRY except dwImageOffset
                    dst.Write(22);                   // ICONDIRENTRY.dwImageOffset

                    // Copy a picture.

                    dst.Write(src, offset, length);
                    // Create an icon from the in-memory file.

                    dst.BaseStream.Seek(0, SeekOrigin.Begin);
                    splitIcons.Add(new Icon(dst.BaseStream));
                }
            }

            return splitIcons.ToList();
        });

        return result;
    }
}
