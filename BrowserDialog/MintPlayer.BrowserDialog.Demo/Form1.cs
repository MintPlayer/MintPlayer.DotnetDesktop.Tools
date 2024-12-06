namespace MintPlayer.BrowserDialog.Test;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private void btnPickBrowser_Click(object sender, EventArgs e)
    {
        var dialog = new BrowserDialog();
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            if (dialog.SelectedBrowser != null)
            {
                MessageBox.Show($"You picked {dialog.SelectedBrowser.Name}.\r\nThe executable path is {dialog.SelectedBrowser.ExecutablePath}\r\nThe version is {dialog.SelectedBrowser.Version}");
            }
            else
            {
                MessageBox.Show("You didn't select a browser");
            }
        }
    }
}
