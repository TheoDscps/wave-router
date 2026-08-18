using WaveRouter.Core.Models;
using WaveRouter.Mvvm;

namespace WaveRouter.ViewModels;

/// <summary>Backs the "route this new app?" popup. Persists the choice (as a rule or an ignore entry)
/// itself via <see cref="RuleListViewModel"/>; raises <see cref="TrackChosen"/> so the caller can also
/// apply routing to the session that's already playing right now. See docs/use-cases/automatic-routing-enforcement.md.</summary>
public sealed class NewAppPromptViewModel : ObservableObject
{
    private readonly RuleListViewModel _ruleList;
    private string _selectedTrack = string.Empty;

    public NewAppPromptViewModel(AudioSessionInfo session, RuleListViewModel ruleList)
    {
        Session = session;
        _ruleList = ruleList;
        AvailableTracks = ruleList.AvailableTracks;

        RouteCommand = new RelayCommand(async _ => await RouteAsync());
        IgnoreCommand = new RelayCommand(async _ => await IgnoreAsync());
    }

    public AudioSessionInfo Session { get; }

    public IReadOnlyList<string> AvailableTracks { get; }

    public string SelectedTrack
    {
        get => _selectedTrack;
        set => SetProperty(ref _selectedTrack, value);
    }

    public RelayCommand RouteCommand { get; }
    public RelayCommand IgnoreCommand { get; }

    /// <summary>Raised once the rule is saved, so the caller can also route the already-playing session.</summary>
    public event EventHandler<string>? TrackChosen;

    /// <summary>Raised in every outcome (routed, ignored) so the caller can close the popup.</summary>
    public event EventHandler? Closed;

    private async Task RouteAsync()
    {
        var track = SelectedTrack.Trim();
        if (track.Length == 0)
        {
            return;
        }

        await _ruleList.AddRuleFromDetectionAsync(Session.ProcessName, track);
        TrackChosen?.Invoke(this, track);
        Closed?.Invoke(this, EventArgs.Empty);
    }

    private async Task IgnoreAsync()
    {
        await _ruleList.IgnoreExecutableAsync(Session.ProcessName);
        Closed?.Invoke(this, EventArgs.Empty);
    }
}
