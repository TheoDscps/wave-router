using WaveRouter.Core.Models;
using WaveRouter.Mvvm;

namespace WaveRouter.ViewModels;

/// <summary>Editable wrapper around a <see cref="Rule"/>. <see cref="OriginalExecutableName"/> tracks identity
/// so a rename can find and replace the right entry in the persisted list.</summary>
public sealed class RuleViewModel : ObservableObject
{
    private string _executableName;
    private string _trackName;

    public RuleViewModel(Rule rule)
    {
        OriginalExecutableName = rule.ExecutableName;
        _executableName = rule.ExecutableName;
        _trackName = rule.TrackName;
    }

    public RuleViewModel()
    {
        OriginalExecutableName = null;
        _executableName = string.Empty;
        _trackName = string.Empty;
    }

    /// <summary>Null for a rule that was never saved yet.</summary>
    public string? OriginalExecutableName { get; private set; }

    public bool IsNew => OriginalExecutableName is null;

    public string ExecutableName
    {
        get => _executableName;
        set => SetProperty(ref _executableName, value);
    }

    public string TrackName
    {
        get => _trackName;
        set => SetProperty(ref _trackName, value);
    }

    public Rule ToRule() => new(ExecutableName.Trim(), TrackName.Trim());

    public void MarkSaved() => OriginalExecutableName = ExecutableName.Trim();
}
