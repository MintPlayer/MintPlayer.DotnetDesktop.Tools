using MintPlayer.BrowserDialog.Presentation;
using System.ComponentModel;

namespace MintPlayer.BrowserDialog;

public partial class BrowserDialog : Form
{
    private readonly BrowserDialogPresenter presenter;
    private BrowserListModel model = BrowserListModel.Empty;

    public BrowserDialog() : this(new BrowserDialogPresenter())
    {
    }

    /// <summary>Creates the dialog over a supplied presenter. Used by tests and by callers that inject their own seams.</summary>
    public BrowserDialog(BrowserDialogPresenter presenter)
    {
        this.presenter = presenter;
        InitializeComponent();
    }

    private async void BrowserDialog_Load(object sender, EventArgs e)
    {
        // Still async void: this is a WinForms event handler, and the framework has no
        // Task to await. BuildModel is documented never to throw, so nothing escapes into
        // an unobserved task; the try/finally below only guards the rendering.
        try
        {
            pnlLoading.Visible = true;
            lvBrowsers.Visible = false;
            lvBrowsers.SuspendLayout();
            lvBrowsers.Items.Clear();

            lvBrowsers.LargeImageList = new ImageList
            {
                ImageSize = new Size(60, 60),
                ColorDepth = ColorDepth.Depth32Bit
            };

            model = await presenter.BuildModel();

            Render();
        }
        finally
        {
            pnlLoading.Visible = false;
            lvBrowsers.Visible = true;
            lvBrowsers.ResumeLayout();
            lvBrowsers.Focus();
        }
    }

    /// <summary>Walks the model onto the list view. Holds no decisions of its own.</summary>
    private void Render()
    {
        var imageCounter = -1;
        foreach (var entry in model.Entries)
        {
            if (entry.Icon != null)
            {
                lvBrowsers.LargeImageList!.Images.Add(entry.Icon);
                imageCounter++;
            }
            else if (entry.Image != null)
            {
                lvBrowsers.LargeImageList!.Images.Add(entry.Image);
                imageCounter++;
            }

            lvBrowsers.Items.Add(new ListViewItem
            {
                Text = entry.Name,
                Tag = entry.ExecutablePath,
                ImageIndex = entry.HasArtwork ? imageCounter : -1,
            });
        }

        if (model.DefaultIndex >= 0 && model.DefaultIndex < lvBrowsers.Items.Count)
        {
            var defaultBrowserListItem = lvBrowsers.Items[model.DefaultIndex];
            defaultBrowserListItem.Focused = defaultBrowserListItem.Selected = true;
        }
    }

    private void BrowserDialog_Shown(object sender, EventArgs e)
    {
        lvBrowsers.Focus();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public PlatformBrowser.Browser? SelectedBrowser
    {
        get
        {
            if (lvBrowsers.SelectedIndices.Count == 0)
            {
                return null;
            }

            return model.Entries[lvBrowsers.SelectedIndices[0]].Browser;
        }
        set
        {
            lvBrowsers.SelectedIndices.Clear();
            if (value == null)
            {
                return;
            }

            var index = model.Entries.ToList().FindIndex(entry => entry.Browser.Name == value.Name);
            if (index >= 0)
            {
                lvBrowsers.SelectedIndices.Add(index);
            }
        }
    }

    private void LvBrowsers_SelectedIndexChanged(object sender, EventArgs e)
    {
        btnOK.Enabled = lvBrowsers.SelectedItems.Count != 0;
    }
}
