
using Bacon.Core.Extensions;
using System.Drawing;

namespace Bacon.Core;

public class BaconIcon : BaconImageSet<int>
{
    public async Task Store(string path, CancellationToken cancellationToken = default)
    {
        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
        await Store(fileStream, cancellationToken);
    }

    public async Task Store(Stream stream, CancellationToken cancellationToken = default)
    {
        stream.Seek(0, SeekOrigin.Begin);
        // Reserved. Must always be 0
        await stream.WriteNumberAsync(0, 2, cancellationToken);
        // Specifies image type: 1 for icon (.ICO) image, 2 for cursor (.CUR) image. Other values are invalid.
        await stream.WriteNumberAsync(1, 2, cancellationToken);
        // The number of images in the file
        await stream.WriteNumberAsync(Images.Count, 2, cancellationToken);

        Bitmap[] imagesRendered = await Task.WhenAll(Images.Select(i => i.Render2(cancellationToken)));

        foreach (var image in Images)
        {
            await stream.WriteNumberAsync(image.Width >= 256 ? 0 : image.Width, 1, cancellationToken);
            await stream.WriteNumberAsync(image.Height >= 256 ? 0 : image.Width, 1, cancellationToken);
            
        }
    }
}
