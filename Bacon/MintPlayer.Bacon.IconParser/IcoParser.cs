using System.Drawing;
using System.Drawing.Imaging;
using MintPlayer.Bacon.IconParser.Enums;

namespace MintPlayer.Bacon.IconParser;

/// <summary>
/// Parser for reading and writing ICO files.
/// ICO file format reference: https://en.wikipedia.org/wiki/ICO_(file_format)
/// </summary>
public static class IcoParser
{
    // ICO file signature
    private const ushort IcoReserved = 0;
    private const ushort IcoType = 1;

    // PNG signature (first 8 bytes)
    private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

    /// <summary>
    /// Read an ICO file and return all images.
    /// </summary>
    /// <param name="path">Path to the ICO file.</param>
    /// <returns>Array of images with their type information.</returns>
    public static ImageWithType[] Read(string path)
    {
        using var stream = File.OpenRead(path);
        return Read(stream);
    }

    /// <summary>
    /// Read an ICO file from a stream and return all images.
    /// </summary>
    /// <param name="stream">Stream containing ICO data.</param>
    /// <returns>Array of images with their type information.</returns>
    public static ImageWithType[] Read(Stream stream)
    {
        using var reader = new BinaryReader(stream);

        // Read ICONDIR header
        var reserved = reader.ReadUInt16();
        var type = reader.ReadUInt16();
        var imageCount = reader.ReadUInt16();

        if (reserved != IcoReserved || type != IcoType)
        {
            throw new InvalidDataException("Invalid ICO file format.");
        }

        // Read ICONDIRENTRY entries
        var entries = new IconDirEntry[imageCount];
        for (int i = 0; i < imageCount; i++)
        {
            entries[i] = new IconDirEntry
            {
                Width = reader.ReadByte(),
                Height = reader.ReadByte(),
                ColorCount = reader.ReadByte(),
                Reserved = reader.ReadByte(),
                Planes = reader.ReadUInt16(),
                BitCount = reader.ReadUInt16(),
                BytesInRes = reader.ReadUInt32(),
                ImageOffset = reader.ReadUInt32()
            };
        }

        // Read image data for each entry
        var images = new List<ImageWithType>();
        foreach (var entry in entries)
        {
            stream.Seek(entry.ImageOffset, SeekOrigin.Begin);
            var imageData = reader.ReadBytes((int)entry.BytesInRes);

            var imageType = IsPng(imageData) ? ImageType.Png : ImageType.Bmp;

            Bitmap bitmap;
            if (imageType == ImageType.Png)
            {
                using var ms = new MemoryStream(imageData);
                bitmap = new Bitmap(ms);
            }
            else
            {
                bitmap = ParseBmpData(imageData, entry);
            }

            images.Add(new ImageWithType(bitmap, imageType));
        }

        return images.ToArray();
    }

    /// <summary>
    /// Write images to an ICO file.
    /// </summary>
    /// <param name="path">Path to write the ICO file.</param>
    /// <param name="images">Images to write.</param>
    public static void Write(string path, ImageWithType[] images)
    {
        using var stream = File.Create(path);
        Write(stream, images);
    }

    /// <summary>
    /// Write images to an ICO stream.
    /// </summary>
    /// <param name="stream">Stream to write to.</param>
    /// <param name="images">Images to write.</param>
    public static void Write(Stream stream, ImageWithType[] images)
    {
        using var writer = new BinaryWriter(stream);

        // Prepare image data
        var imageDataList = new List<byte[]>();
        foreach (var image in images)
        {
            byte[] data;
            if (image.Type == ImageType.Png)
            {
                using var ms = new MemoryStream();
                image.Image.Save(ms, ImageFormat.Png);
                data = ms.ToArray();
            }
            else
            {
                data = CreateBmpData(image.Image);
            }
            imageDataList.Add(data);
        }

        // Write ICONDIR header
        writer.Write(IcoReserved);
        writer.Write(IcoType);
        writer.Write((ushort)images.Length);

        // Calculate offsets (header = 6 bytes, each entry = 16 bytes)
        var headerSize = 6 + (images.Length * 16);
        var currentOffset = (uint)headerSize;

        // Write ICONDIRENTRY for each image
        for (int i = 0; i < images.Length; i++)
        {
            var image = images[i];
            var imageData = imageDataList[i];

            // Width and Height: 0 means 256
            var width = (byte)(image.Width >= 256 ? 0 : image.Width);
            var height = (byte)(image.Height >= 256 ? 0 : image.Height);

            writer.Write(width);
            writer.Write(height);
            writer.Write((byte)0); // ColorCount (0 for PNG, or 256+ colors)
            writer.Write((byte)0); // Reserved
            writer.Write((ushort)1); // Planes
            writer.Write((ushort)32); // BitCount (32 for ARGB)
            writer.Write((uint)imageData.Length); // BytesInRes
            writer.Write(currentOffset); // ImageOffset

            currentOffset += (uint)imageData.Length;
        }

        // Write image data
        foreach (var imageData in imageDataList)
        {
            writer.Write(imageData);
        }
    }

    private static bool IsPng(byte[] data)
    {
        if (data.Length < 8) return false;
        for (int i = 0; i < 8; i++)
        {
            if (data[i] != PngSignature[i]) return false;
        }
        return true;
    }

    private static Bitmap ParseBmpData(byte[] data, IconDirEntry entry)
    {
        // BMP data in ICO files is stored without BITMAPFILEHEADER
        // and the height is doubled (includes mask)
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);

        // Read BITMAPINFOHEADER
        var headerSize = reader.ReadUInt32();
        var width = reader.ReadInt32();
        var height = reader.ReadInt32(); // Doubled height (image + mask)
        var planes = reader.ReadUInt16();
        var bitCount = reader.ReadUInt16();
        var compression = reader.ReadUInt32();
        var imageSize = reader.ReadUInt32();
        var xPelsPerMeter = reader.ReadInt32();
        var yPelsPerMeter = reader.ReadInt32();
        var colorsUsed = reader.ReadUInt32();
        var colorsImportant = reader.ReadUInt32();

        // Actual image height is half the stored height
        var actualHeight = height / 2;
        var actualWidth = entry.Width == 0 ? 256 : entry.Width;
        actualHeight = entry.Height == 0 ? 256 : entry.Height;

        var bitmap = new Bitmap(actualWidth, actualHeight, PixelFormat.Format32bppArgb);

        // For 32-bit images, read pixel data directly
        if (bitCount == 32)
        {
            var stride = actualWidth * 4;
            ms.Seek(headerSize, SeekOrigin.Begin);

            // BMP stores rows bottom-to-top
            for (int y = actualHeight - 1; y >= 0; y--)
            {
                for (int x = 0; x < actualWidth; x++)
                {
                    var b = reader.ReadByte();
                    var g = reader.ReadByte();
                    var r = reader.ReadByte();
                    var a = reader.ReadByte();
                    bitmap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
            }
        }
        else
        {
            // For other bit depths, try to load via GDI+
            // This is a simplified approach; full implementation would handle all bit depths
            try
            {
                // Create a proper BMP file in memory
                using var bmpStream = new MemoryStream();
                using var bmpWriter = new BinaryWriter(bmpStream);

                // Write BITMAPFILEHEADER
                bmpWriter.Write((ushort)0x4D42); // 'BM'
                bmpWriter.Write(14 + data.Length); // File size
                bmpWriter.Write((ushort)0); // Reserved
                bmpWriter.Write((ushort)0); // Reserved
                bmpWriter.Write(14 + (int)headerSize); // Offset to pixel data

                // Fix height in header (use actual height, not doubled)
                var fixedData = (byte[])data.Clone();
                Buffer.BlockCopy(BitConverter.GetBytes(actualHeight), 0, fixedData, 8, 4);

                bmpWriter.Write(fixedData);
                bmpStream.Seek(0, SeekOrigin.Begin);

                using var tempBitmap = new Bitmap(bmpStream);
                using var graphics = Graphics.FromImage(bitmap);
                graphics.DrawImage(tempBitmap, 0, 0, actualWidth, actualHeight);
            }
            catch
            {
                // If all else fails, return a blank bitmap
            }
        }

        return bitmap;
    }

    private static byte[] CreateBmpData(Bitmap bitmap)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        var width = bitmap.Width;
        var height = bitmap.Height;

        // Write BITMAPINFOHEADER
        writer.Write((uint)40); // Header size
        writer.Write(width); // Width
        writer.Write(height * 2); // Height (doubled for mask)
        writer.Write((ushort)1); // Planes
        writer.Write((ushort)32); // BitCount
        writer.Write((uint)0); // Compression
        writer.Write((uint)(width * height * 4 + width * height / 8)); // Image size
        writer.Write(0); // XPelsPerMeter
        writer.Write(0); // YPelsPerMeter
        writer.Write((uint)0); // ColorsUsed
        writer.Write((uint)0); // ColorsImportant

        // Write pixel data (bottom-to-top)
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                writer.Write(pixel.B);
                writer.Write(pixel.G);
                writer.Write(pixel.R);
                writer.Write(pixel.A);
            }
        }

        // Write AND mask (all zeros for 32-bit = no masking)
        var maskRowSize = ((width + 31) / 32) * 4; // Padded to 4 bytes
        var maskData = new byte[maskRowSize * height];
        writer.Write(maskData);

        return ms.ToArray();
    }

    private struct IconDirEntry
    {
        public byte Width;
        public byte Height;
        public byte ColorCount;
        public byte Reserved;
        public ushort Planes;
        public ushort BitCount;
        public uint BytesInRes;
        public uint ImageOffset;
    }
}
