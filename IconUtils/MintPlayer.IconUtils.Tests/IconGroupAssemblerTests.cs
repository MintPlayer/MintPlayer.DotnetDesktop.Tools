namespace MintPlayer.IconUtils.Tests;

/// <summary>
/// Exercises the GRPICONDIR -> ICONDIR conversion against synthetic buffers. This is the
/// half of icon extraction that can actually be wrong: a 14-byte group entry has to become
/// a 16-byte file entry, sharing its first 8 bytes and replacing the trailing 2-byte
/// resource id with a 4-byte length and a 4-byte offset.
/// </summary>
public class IconGroupAssemblerTests
{
    /// <summary>Builds a GRPICONDIR describing <paramref name="images"/>, in the Win32 layout.</summary>
    private static byte[] BuildGroup(params (ushort Id, int DeclaredLength)[] images)
    {
        var buffer = new byte[6 + (14 * images.Length)];

        // ICONDIR: reserved, type = 1 (icon), count
        BitConverter.GetBytes((ushort)0).CopyTo(buffer, 0);
        BitConverter.GetBytes((ushort)1).CopyTo(buffer, 2);
        BitConverter.GetBytes((ushort)images.Length).CopyTo(buffer, 4);

        for (var i = 0; i < images.Length; i++)
        {
            var offset = 6 + (14 * i);
            buffer[offset + 0] = 32;                                            // width
            buffer[offset + 1] = 32;                                            // height
            buffer[offset + 2] = 0;                                             // colour count
            buffer[offset + 3] = 0;                                             // reserved
            BitConverter.GetBytes((ushort)1).CopyTo(buffer, offset + 4);        // planes
            BitConverter.GetBytes((ushort)32).CopyTo(buffer, offset + 6);       // bit count
            BitConverter.GetBytes(images[i].DeclaredLength).CopyTo(buffer, offset + 8);
            BitConverter.GetBytes(images[i].Id).CopyTo(buffer, offset + 12);
        }

        return buffer;
    }

    private static byte[] Image(byte fill, int length) => [.. Enumerable.Repeat(fill, length)];

    [Fact]
    public void Reads_the_image_count_from_the_header()
    {
        Assert.Equal(3, IconGroupAssembler.GetImageCount(BuildGroup((1, 4), (2, 4), (3, 4))));
    }

    [Fact]
    public void Reads_each_image_resource_id()
    {
        var group = BuildGroup((7, 4), (9, 4));

        Assert.Equal(7, IconGroupAssembler.GetImageResourceId(group, 0));
        Assert.Equal(9, IconGroupAssembler.GetImageResourceId(group, 1));
    }

    [Fact]
    public void Rejects_a_buffer_too_short_to_be_a_header()
    {
        Assert.Throws<ArgumentException>(() => IconGroupAssembler.GetImageCount([0, 0, 1]));
    }

    [Fact]
    public void Rejects_a_header_declaring_more_images_than_the_buffer_holds()
    {
        var truncated = BuildGroup((1, 4), (2, 4))[..10];

        Assert.Throws<ArgumentException>(() => IconGroupAssembler.BuildIconFile(truncated, _ => [1]));
    }

    [Fact]
    public void Rejects_an_index_past_the_end()
    {
        var group = BuildGroup((1, 4));

        Assert.Throws<ArgumentOutOfRangeException>(() => IconGroupAssembler.GetImageResourceId(group, 1));
    }

    [Fact]
    public void Copies_the_six_byte_header_verbatim()
    {
        var group = BuildGroup((1, 4));

        var ico = IconGroupAssembler.BuildIconFile(group, _ => Image(0xAA, 4));

        Assert.Equal(group[..6], ico[..6]);
    }

    [Fact]
    public void Writes_a_sixteen_byte_entry_per_image_sharing_the_first_eight_bytes()
    {
        var group = BuildGroup((1, 4), (2, 4));

        var ico = IconGroupAssembler.BuildIconFile(group, _ => Image(0xAA, 4));

        for (var i = 0; i < 2; i++)
        {
            var groupEntry = group.AsSpan(6 + (14 * i), 8).ToArray();
            var fileEntry = ico.AsSpan(6 + (16 * i), 8).ToArray();
            Assert.Equal(groupEntry, fileEntry);
        }
    }

    [Fact]
    public void Lays_images_out_back_to_back_after_the_directory()
    {
        var group = BuildGroup((1, 3), (2, 5));
        var images = new Dictionary<ushort, byte[]>
        {
            [1] = Image(0x11, 3),
            [2] = Image(0x22, 5),
        };

        var ico = IconGroupAssembler.BuildIconFile(group, id => images[id]);

        var directoryEnd = 6 + (16 * 2);

        // First entry: length 3 at the end of the directory.
        Assert.Equal(3, BitConverter.ToInt32(ico, 6 + 8));
        Assert.Equal(directoryEnd, BitConverter.ToInt32(ico, 6 + 12));

        // Second entry: length 5, immediately after the first image.
        Assert.Equal(5, BitConverter.ToInt32(ico, 6 + 16 + 8));
        Assert.Equal(directoryEnd + 3, BitConverter.ToInt32(ico, 6 + 16 + 12));

        Assert.Equal(images[1], ico[directoryEnd..(directoryEnd + 3)]);
        Assert.Equal(images[2], ico[(directoryEnd + 3)..(directoryEnd + 8)]);
        Assert.Equal(directoryEnd + 8, ico.Length);
    }

    [Fact]
    public void Uses_the_real_image_length_when_the_resource_declares_a_different_one()
    {
        // A resource whose declared dwBytesInRes disagrees with the bytes actually returned
        // must still produce a coherent file: the declared value is only a sizing hint.
        var group = BuildGroup((1, 999));

        var ico = IconGroupAssembler.BuildIconFile(group, _ => Image(0x33, 4));

        Assert.Equal(4, BitConverter.ToInt32(ico, 6 + 8));
        Assert.Equal(6 + 16 + 4, ico.Length);
    }

    [Fact]
    public void Fetches_each_image_by_the_id_in_its_group_entry()
    {
        var group = BuildGroup((42, 2), (7, 2));
        var requested = new List<ushort>();

        IconGroupAssembler.BuildIconFile(group, id =>
        {
            requested.Add(id);
            return Image(0, 2);
        });

        Assert.Equal<ushort>([42, 7], requested);
    }

    [Fact]
    public void Fails_loudly_when_an_image_resource_is_missing()
    {
        var group = BuildGroup((1, 4));

        Assert.Throws<InvalidOperationException>(() => IconGroupAssembler.BuildIconFile(group, _ => null!));
    }

    [Fact]
    public void Produces_an_empty_directory_for_a_group_with_no_images()
    {
        var ico = IconGroupAssembler.BuildIconFile(BuildGroup(), _ => []);

        Assert.Equal(6, ico.Length);
        Assert.Equal(0, BitConverter.ToUInt16(ico, 4));
    }
}
