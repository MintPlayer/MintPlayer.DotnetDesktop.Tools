using MintPlayer.IconUtils;
using System.Collections.ObjectModel;

namespace MintPlayer.BrowserDialog;

public partial class BrowserDialog : Form
{
    public BrowserDialog()
    {
        InitializeComponent();
    }

    private async void BrowserDialog_Load(object sender, EventArgs e)
    {
        try
        {
            pnlLoading.Visible = true;
            lvBrowsers.Visible = false;


            // Suspend the drawing of the listview
            lvBrowsers.SuspendLayout();

            // Remove all items from the listview
            lvBrowsers.Items.Clear();

            // Assign a new imagelist
            lvBrowsers.LargeImageList = new ImageList
            {
                ImageSize = new Size(60, 60),
                ColorDepth = ColorDepth.Depth32Bit
            };

            // Get all browsers on the system
            browsers = await PlatformBrowser.PlatformBrowser.GetInstalledBrowsers();

            // Loop through all browsers
            var imageCounter = -1;
            for (var i = 0; i < browsers.Count; i++)
            {
                var browser = browsers[i];

                if (!string.IsNullOrEmpty(browser.IconPath))
                {
                    if (new[] { ".ico", ".cur", ".exe" }.Contains(Path.GetExtension(browser.IconPath)))
                    {
                        // Get image
                        var split = await IconExtractor.Split(browser.IconPath);
                        var icon = split[browser.IconIndex < 0 ? 0 : browser.IconIndex];
                        var icons = await IconExtractor.ExtractImagesFromIcon(icon);
                        var largestSize = icons.Max(i => i.Width);
                        var largestIcon = icons.LastOrDefault(i => i.Width == largestSize);
                        if (largestIcon != null)
                        {
                            lvBrowsers.LargeImageList.Images.Add(largestIcon);
                            imageCounter++;
                        }
                    }
                    else
                    {
                        var image = Image.FromFile(browser.IconPath);
                        lvBrowsers.LargeImageList.Images.Add(image);
                        imageCounter++;
                    }
                }

                lvBrowsers.Items.Add(new ListViewItem
                {
                    Text = browser.Name,
                    Tag = browser.ExecutablePath.Trim('\"'),
                    ImageIndex = string.IsNullOrEmpty(browser.IconPath) ? -1 : imageCounter,
                });
            }

            // Get default browser
            defaultBrowser = await PlatformBrowser.PlatformBrowser.GetDefaultBrowser(browsers.ToList(), PlatformBrowser.Enums.EProtocolType.Http);

            // Select default browser
            if (browsers.Contains(defaultBrowser!))
            {
                var defaultBrowserListItem = lvBrowsers.Items[
                    browsers.IndexOf(
                        browsers.FirstOrDefault(b => b.ExecutablePath == defaultBrowser?.ExecutablePath)!
                    )
                ];
                defaultBrowserListItem.Focused = defaultBrowserListItem.Selected = true;
            }
        }
        catch (Exception)
        {
            // Don't interrupt the dialog
        }
        finally
        {
            pnlLoading.Visible = false;
            lvBrowsers.Visible = true;
            lvBrowsers.ResumeLayout();
            lvBrowsers.Focus();
        }
    }

    private void BrowserDialog_Shown(object sender, EventArgs e)
    {
        lvBrowsers.Focus();
    }

    private ReadOnlyCollection<PlatformBrowser.Browser> browsers = new ReadOnlyCollection<PlatformBrowser.Browser>(new List<PlatformBrowser.Browser>());
    private PlatformBrowser.Browser? defaultBrowser;
    public PlatformBrowser.Browser? SelectedBrowser
    {
        get
        {
            if (lvBrowsers.SelectedIndices.Count == 0)
            {
                return null;
            }
            else
            {
                return browsers[lvBrowsers.SelectedIndices[0]];
            }
        }
        set
        {
            lvBrowsers.SelectedIndices.Clear();
            if (value == null)
            {
                return;
            }

            if (browsers.Any(b => b.Name == value.Name))
            {
                lvBrowsers.SelectedIndices.Add(browsers.IndexOf(value));
            }
        }
    }

    private void LvBrowsers_SelectedIndexChanged(object sender, EventArgs e)
    {
        btnOK.Enabled = lvBrowsers.SelectedItems.Count != 0;
    }

}
