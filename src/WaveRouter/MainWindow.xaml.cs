using System.ComponentModel;
using WaveRouter.ViewModels;

namespace WaveRouter;

public partial class MainWindow : System.Windows.Window
{
    public MainWindow(RuleListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    /// <summary>Closing the window hides it — the app keeps running in the tray until "Quitter" is used.</summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}
