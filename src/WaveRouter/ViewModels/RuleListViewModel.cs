using System.Collections.ObjectModel;
using WaveRouter.Core.Abstractions;
using WaveRouter.Core.Errors;
using WaveRouter.Mvvm;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace WaveRouter.ViewModels;

/// <summary>Backs the rule list + detail panel in <see cref="MainWindow"/>.
/// See docs/use-cases/rule-management.md and docs/use-cases/rule-persistence.md.</summary>
public sealed class RuleListViewModel : ObservableObject
{
    private readonly IRuleRepository _repository;
    private RuleViewModel? _selectedRule;
    private string? _validationError;
    private string? _statusMessage;

    public RuleListViewModel(IRuleRepository repository, RuleLoadResult initialLoad)
    {
        _repository = repository;
        Rules = new ObservableCollection<RuleViewModel>(initialLoad.Rules.Select(r => new RuleViewModel(r)));
        _statusMessage = initialLoad.Warning;

        AddRuleCommand = new RelayCommand(_ => AddRule());
        SaveRuleCommand = new RelayCommand(async _ => await SaveSelectedRuleAsync());
        DeleteRuleCommand = new RelayCommand(async _ => await DeleteSelectedRuleAsync());
    }

    public ObservableCollection<RuleViewModel> Rules { get; }

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
            await _repository.SaveAsync(savedRules);
            StatusMessage = null;
        }
        catch (RulePersistenceException ex)
        {
            StatusMessage = ex.Message;
        }
    }
}
