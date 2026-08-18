using System.Collections.ObjectModel;
using WaveRouter.Core.Abstractions;
using WaveRouter.Core.Errors;
using WaveRouter.Core.Models;
using WaveRouter.Core.Rules;
using WaveRouter.Mvvm;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace WaveRouter.ViewModels;

/// <summary>Backs the rule list + detail panel in <see cref="MainWindow"/>, and the rule-creation half of
/// the new-app prompt. See docs/use-cases/rule-management.md and docs/use-cases/rule-persistence.md.</summary>
public sealed class RuleListViewModel : ObservableObject
{
    private readonly IRuleRepository _repository;
    private readonly List<string> _ignoredExecutables;
    private RuleViewModel? _selectedRule;
    private string? _validationError;
    private string? _statusMessage;

    public RuleListViewModel(IRuleRepository repository, ITrackProvider trackProvider, RuleStoreLoadResult initialLoad)
    {
        _repository = repository;
        Rules = new ObservableCollection<RuleViewModel>(initialLoad.Store.Rules.Select(r => new RuleViewModel(r)));
        _ignoredExecutables = [.. initialLoad.Store.IgnoredExecutables];
        _statusMessage = initialLoad.Warning;
        AvailableTracks = trackProvider.GetAvailableTracks();

        AddRuleCommand = new RelayCommand(_ => AddRule());
        SaveRuleCommand = new RelayCommand(async _ => await SaveSelectedRuleAsync());
        DeleteRuleCommand = new RelayCommand(async _ => await DeleteSelectedRuleAsync());
    }

    public ObservableCollection<RuleViewModel> Rules { get; }

    /// <summary>Track names read from active Wave Link output devices — empty if Wave Link isn't running.
    /// The track field stays free text either way (see docs/use-cases/read-wave-link-tracks.md).</summary>
    public IReadOnlyList<string> AvailableTracks { get; }

    public IReadOnlyList<string> IgnoredExecutables => _ignoredExecutables;

    public RuleViewModel? SelectedRule
    {
        get => _selectedRule;
        set => SetProperty(ref _selectedRule, value);
    }

    /// <summary>Inline validation message for the detail panel (empty fields).</summary>
    public string? ValidationError
    {
        get => _validationError;
        private set => SetProperty(ref _validationError, value);
    }

    /// <summary>Persistence warnings/errors surfaced to the user (corrupted store, write failure).</summary>
    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public RelayCommand AddRuleCommand { get; }
    public RelayCommand SaveRuleCommand { get; }
    public RelayCommand DeleteRuleCommand { get; }

    private void AddRule()
    {
        var rule = new RuleViewModel();
        Rules.Add(rule);
        SelectedRule = rule;
    }

    /// <summary>Called from the new-app prompt once the user picked a track — creates and persists the
    /// rule directly, no inline validation needed since both fields are already known-good.</summary>
    public async Task AddRuleFromDetectionAsync(string executableName, string trackName)
    {
        var rule = new RuleViewModel { ExecutableName = executableName, TrackName = trackName };
        rule.MarkSaved();
        Rules.Add(rule);
        await PersistAsync();
    }

    /// <summary>Called from the new-app prompt when the user dismisses it — never prompt for this
    /// executable again.</summary>
    public async Task IgnoreExecutableAsync(string executableName)
    {
        var normalized = RuleMatcher.NormalizeExecutableName(executableName);
        if (_ignoredExecutables.Any(e => string.Equals(RuleMatcher.NormalizeExecutableName(e), normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _ignoredExecutables.Add(executableName);
        await PersistAsync();
    }

    private async Task SaveSelectedRuleAsync()
    {
        var rule = SelectedRule;
        if (rule is null)
        {
            return;
        }

        ValidationError = null;

        var executableName = rule.ExecutableName.Trim();
        var trackName = rule.TrackName.Trim();
        if (executableName.Length == 0 || trackName.Length == 0)
        {
            ValidationError = "The executable and the track are both required.";
            return;
        }

        var conflict = Rules.FirstOrDefault(other =>
            other != rule && string.Equals(other.ExecutableName.Trim(), executableName, StringComparison.OrdinalIgnoreCase));

        if (conflict is not null)
        {
            var overwrite = MessageBox.Show(
                $"A rule for \"{executableName}\" already exists. Overwrite it?",
                "Duplicate rule",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (overwrite != MessageBoxResult.Yes)
            {
                return;
            }

            Rules.Remove(conflict);
        }

        rule.MarkSaved();
        await PersistAsync();
    }

    private async Task DeleteSelectedRuleAsync()
    {
        var rule = SelectedRule;
        if (rule is null)
        {
            return;
        }

        var wasPersisted = !rule.IsNew;
        Rules.Remove(rule);
        SelectedRule = Rules.FirstOrDefault();

        if (wasPersisted)
        {
            await PersistAsync();
        }
    }

    private async Task PersistAsync()
    {
        try
        {
            var savedRules = Rules.Where(r => !r.IsNew).Select(r => r.ToRule()).ToList();
            await _repository.SaveAsync(new RuleStore(savedRules, _ignoredExecutables));
            StatusMessage = null;
        }
        catch (RulePersistenceException ex)
        {
            StatusMessage = ex.Message;
        }
    }
}
