using System.ComponentModel;

namespace WaveRouter;

public partial class MainWindow : System.Windows.Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>Closing the window hides it — the app keeps running in the tray until "Quitter" is used.</summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}
