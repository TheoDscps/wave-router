using System.Text.Json;
using WaveRouter.Core.Abstractions;
using WaveRouter.Core.Models;

namespace WaveRouter.Infrastructure.Persistence;

/// <summary>Stores app preferences as JSON under %AppData%/WaveRouter/settings.json — a separate file
/// from rules.json since they're a different concern with a different lifecycle.</summary>
public sealed class JsonAppSettingsRepository : IAppSettingsRepository
{
    private readonly string _filePath;
    private readonly string _tempPath;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public JsonAppSettingsRepository()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WaveRouter");
        Directory.CreateDirectory(directory);

        _filePath = Path.Combine(directory, "settings.json");
        _tempPath = Path.Combine(directory, "settings.json.tmp");
    }

    public async Task<AppSettings> LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            return AppSettings.Default;
        }

        try
        {
            await using var stream = File.OpenRead(_filePath);
            return await JsonSerializer.DeserializeAsync<AppSettings>(stream) ?? AppSettings.Default;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return AppSettings.Default;
        }
    }

    public async Task SaveAsync(AppSettings settings)
    {
        await _writeLock.WaitAsync();
        try
        {
            await using (var stream = File.Create(_tempPath))
            {
                await JsonSerializer.SerializeAsync(stream, settings, new JsonSerializerOptions { WriteIndented = true });
            }

            File.Move(_tempPath, _filePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Settings are low-stakes and re-derivable from defaults — unlike rules, not worth surfacing
            // a persistent error banner for. Best-effort: the in-memory choice still applies this session.
        }
        finally
        {
            _writeLock.Release();
        }
    }
}
