using System.ComponentModel;
using WaveRouter.ViewModels;

namespace WaveRouter;

public partial class NewAppPromptWindow : System.Windows.Window
{
    private readonly NewAppPromptViewModel _viewModel;
    private bool _handled;

    public NewAppPromptWindow(NewAppPromptViewModel viewModel)
    {
        InitializeComponent();
        WindowChromeHelper.ApplyDarkTitleBar(this);
        DataContext = _viewModel = viewModel;
        _viewModel.Closed += (_, _) =>
        {
            if (_handled)
            {
                return;
            }

            _handled = true;
            Close();
        };
    }

    /// <summary>Closing without an explicit choice (the X button) counts as "ignore" — never ask again for this app.</summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        if (_handled)
        {
            return;
        }

        _handled = true;
        _viewModel.IgnoreCommand.Execute(null); // fire-and-forget: the window closes immediately, the save continues in the background
    }
}
