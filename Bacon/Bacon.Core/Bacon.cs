using System.Drawing;
using System.Drawing.Imaging;

namespace MintPlayer.Bacon;

public class Bacon
{
    public MintPlayer.ObservableCollection.ObservableCollection<BaconImage> Images { get; } = new();

    public Icon ToIcon()
    {
        if (!Images.Any()) throw new InvalidOperationException("No images available");
        // Build ICO header and directory
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write((short)0); // Reserved
        bw.Write((short)1); // Type (1=icon)
        bw.Write((short)Images.Count); // Count

        long dirEntryPos = ms.Position;
        // Reserve space for directory entries
        bw.Write(new byte[16 * Images.Count]);

        var imageDataList = new List<(int index, byte[] data, int width, int height, int colorCount, int planes, int bpp)>();
        for (int i = 0; i < Images.Count; i++)
        {
            var rendered = Images[i].Render();
            // Save as PNG (modern ICO supports embedded PNG for >= 256 colors)
            using var imgMs = new MemoryStream();
            rendered.Save(imgMs, ImageFormat.Png);
            var data = imgMs.ToArray();
            imageDataList.Add((i, data, rendered.Width, rendered.Height, 0, 1, 32));
        }

        var directoryEntries = new List<byte[]>();
        foreach (var item in imageDataList)
        {
            // Prepare directory entry
            var entry = new byte[16];
            entry[0] = (byte)(item.width >= 256 ? 0 : item.width);
            entry[1] = (byte)(item.height >= 256 ? 0 : item.height);
            entry[2] = (byte)item.colorCount; // color count
            entry[3] = 0; // reserved
            BitConverter.GetBytes((short)item.planes).CopyTo(entry, 4);
            BitConverter.GetBytes((short)item.bpp).CopyTo(entry, 6);
            BitConverter.GetBytes(item.data.Length).CopyTo(entry, 8);
            BitConverter.GetBytes((int)ms.Position).CopyTo(entry, 12);

            // Write image data
            bw.Write(item.data);
            directoryEntries.Add(entry);
        }

        // Seek back and write directory entries
        ms.Seek(dirEntryPos, SeekOrigin.Begin);
        foreach (var entry in directoryEntries) bw.Write(entry);
        ms.Seek(0, SeekOrigin.Begin);
        return new Icon(ms);
    }
}
