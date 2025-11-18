using System;
using System.Windows.Forms;

namespace MintPlayer.Bacon.Editor.Demo;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new FrmDemo());
    }
}
