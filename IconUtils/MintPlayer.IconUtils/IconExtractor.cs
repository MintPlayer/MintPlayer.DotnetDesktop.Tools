using MintPlayer.IconUtils.Native;
using System.Drawing;

namespace MintPlayer.IconUtils;

/// <inheritdoc cref="IIconExtractor" />
/// <remarks>
/// Was a static class. It is an instance class now so the native module reader can be
/// substituted, which is what makes the resource parsing in <see cref="IconGroupAssembler"/>
/// reachable from a test.
/// </remarks>
public class IconExtractor : IIconExtractor
{
    private readonly INativeModuleResources moduleResources;

    /// <summary>Creates an extractor backed by the real kernel32 resource APIs.</summary>
    public IconExtractor() : this(new NativeModuleResources())
    {
    }

    /// <summary>Creates an extractor over the supplied native-resource seam.</summary>
    public IconExtractor(INativeModuleResources moduleResources)
    {
        this.moduleResources = moduleResources;
    }

    /// <inheritdoc />
    public async Task<List<Icon>> Split(string filename)
    {
        if (string.IsNullOrEmpty(filename))
        {
            throw new ArgumentNullException(nameof(filename));
        }

        if (!File.Exists(filename))
        {
            throw new FileNotFoundException("File not found", filename);
        }

        switch (Path.GetExtension(filename))
        {
            case ".exe":
                return await ExtractIconsFromExe(filename);
            case ".ico":
            case ".cur":
                return await Task.Run(async () =>
                {
                    var icon = new Icon(filename);
                    return await ExtractImagesFromIcon(icon);
                });
            default:
                throw new InvalidOperationException(@"Input file must have one of following extensions: "".exe"", "".ico"", "".cur""");
        }
    }

    /// <inheritdoc />
    public async Task<List<Icon>> ExtractImagesFromIcon(Icon icon)
    {
        if (icon == null)
        {
            throw new ArgumentNullException(nameof(icon));
        }

        return await Utils.IconUtils.Split(icon);
    }

    private async Task<List<Icon>> ExtractIconsFromExe(string exeFileName)
    {
        return await Task.Run(() =>
        {
            using var module = moduleResources.Open(exeFileName);

            var icons = new List<Icon>();
            foreach (var groupName in module.EnumerateIconGroupNames())
            {
                var groupData = module.GetIconGroupData(groupName);
                var icoBytes = IconGroupAssembler.BuildIconFile(groupData, id => module.GetIconImageData(id));

                using var stream = new MemoryStream(icoBytes);
                icons.Add(new Icon(stream));
            }

            return icons;
        });
    }
}
