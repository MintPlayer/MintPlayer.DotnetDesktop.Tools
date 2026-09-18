namespace MintPlayer.IconUtils;

/// <summary>
/// Turns a Win32 RT_GROUP_ICON resource plus its RT_ICON images into the bytes of a
/// standalone .ico file.
/// </summary>
/// <remarks>
/// Pure managed byte manipulation, deliberately kept out of the P/Invoke adapter: this is
/// the part that can actually be wrong, and it can be exercised against synthetic buffers
/// without a PE file on disk.
///
/// Layout reference: http://msdn.microsoft.com/en-us/library/ms997538.aspx
/// A GRPICONDIRENTRY is 14 bytes and an on-disk ICONDIRENTRY is 16; the first 8 bytes are
/// identical, and the last 8 differ - the group entry ends with a 2-byte resource id,
/// the file entry with a 4-byte length and a 4-byte offset. That 14-vs-16 asymmetry is
/// the whole reason this conversion exists.
/// </remarks>
public static class IconGroupAssembler
{
    private const int IconDirSize = 6;
    private const int GroupEntrySize = 14;
    private const int FileEntrySize = 16;

    /// <summary>Number of images described by a GRPICONDIR.</summary>
    /// <param name="groupData">Raw RT_GROUP_ICON bytes.</param>
    public static int GetImageCount(byte[] groupData)
    {
        ArgumentNullException.ThrowIfNull(groupData);
        if (groupData.Length < IconDirSize)
        {
            throw new ArgumentException($"A GRPICONDIR is at least {IconDirSize} bytes; got {groupData.Length}.", nameof(groupData));
        }

        return BitConverter.ToUInt16(groupData, 4);
    }

    /// <summary>Resource id of the image at <paramref name="index"/> within the group.</summary>
    public static ushort GetImageResourceId(byte[] groupData, int index)
    {
        var count = GetImageCount(groupData);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, count);

        return BitConverter.ToUInt16(groupData, IconDirSize + (GroupEntrySize * index) + 12);
    }

    /// <summary>
    /// Assembles a complete .ico file from a group resource and a way to fetch each image.
    /// </summary>
    /// <param name="groupData">Raw RT_GROUP_ICON bytes.</param>
    /// <param name="getImageData">Fetches the RT_ICON bytes for a resource id.</param>
    public static byte[] BuildIconFile(byte[] groupData, Func<ushort, byte[]> getImageData)
    {
        ArgumentNullException.ThrowIfNull(getImageData);

        var count = GetImageCount(groupData);
        var required = IconDirSize + (GroupEntrySize * count);
        if (groupData.Length < required)
        {
            throw new ArgumentException(
                $"GRPICONDIR declares {count} images, which needs {required} bytes; got {groupData.Length}.",
                nameof(groupData));
        }

        // Size the buffer from the declared lengths so the stream is not repeatedly grown.
        // It is a hint only: the real images are written at their actual length below, so a
        // resource whose declared size lies produces a correct file either way.
        var declaredTotal = IconDirSize + (FileEntrySize * count);
        for (var i = 0; i < count; i++)
        {
            declaredTotal += BitConverter.ToInt32(groupData, IconDirSize + (GroupEntrySize * i) + 8);
        }

        using var stream = new MemoryStream(declaredTotal);
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            // The 6-byte ICONDIR header is identical in both layouts.
            writer.Write(groupData, 0, IconDirSize);

            var imageOffset = IconDirSize + (FileEntrySize * count);
            for (var i = 0; i < count; i++)
            {
                var id = GetImageResourceId(groupData, i);
                var image = getImageData(id)
                    ?? throw new InvalidOperationException($"No RT_ICON resource data for image id {id}.");

                writer.Seek(IconDirSize + (FileEntrySize * i), SeekOrigin.Begin);
                // First 8 bytes of GRPICONDIRENTRY and ICONDIRENTRY are identical.
                writer.Write(groupData, IconDirSize + (GroupEntrySize * i), 8);
                writer.Write(image.Length);  // ICONDIRENTRY.dwBytesInRes
                writer.Write(imageOffset);   // ICONDIRENTRY.dwImageOffset

                writer.Seek(imageOffset, SeekOrigin.Begin);
                writer.Write(image, 0, image.Length);

                imageOffset += image.Length;
            }
        }

        return stream.ToArray();
    }
}
