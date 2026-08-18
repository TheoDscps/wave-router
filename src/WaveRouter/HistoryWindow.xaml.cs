using WaveRouter.ViewModels;

namespace WaveRouter;

public partial class HistoryWindow : System.Windows.Window
{
    public HistoryWindow(HistoryViewModel viewModel)
    {
        InitializeComponent();
        WindowChromeHelper.ApplyDarkTitleBar(this);
        DataContext = viewModel;
    }
}
