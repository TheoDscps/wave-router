using WaveRouter.ViewModels;

namespace WaveRouter;

public partial class SettingsWindow : System.Windows.Window
{
    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        WindowChromeHelper.ApplyDarkTitleBar(this);
        DataContext = viewModel;
    }

    private void OnCloseClicked(object sender, System.Windows.RoutedEventArgs e) => Close();
}
