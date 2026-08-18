using WaveRouter.Core.Models;

namespace WaveRouter.Core.Abstractions;

public interface IAppSettingsRepository
{
    /// <summary>Returns <see cref="AppSettings.Default"/> on first run (no file yet) or if the file is corrupted.</summary>
    Task<AppSettings> LoadAsync();

    Task SaveAsync(AppSettings settings);
}
