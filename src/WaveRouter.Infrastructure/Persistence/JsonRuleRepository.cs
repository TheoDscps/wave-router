using System.Text.Json;
using WaveRouter.Core.Abstractions;
using WaveRouter.Core.Errors;
using WaveRouter.Core.Models;

namespace WaveRouter.Infrastructure.Persistence;

/// <summary>Stores rules as JSON under %AppData%/WaveRouter/rules.json. Writes are atomic (temp file + move)
/// so an unclean shutdown mid-write can't corrupt the store. See docs/use-cases/rule-persistence.md.</summary>
public sealed class JsonRuleRepository : IRuleRepository
{
    private readonly string _filePath;
    private readonly string _backupPath;
    private readonly string _tempPath;

    public JsonRuleRepository()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WaveRouter");
        Directory.CreateDirectory(directory);

        _filePath = Path.Combine(directory, "rules.json");
        _backupPath = Path.Combine(directory, "rules.json.bak");
        _tempPath = Path.Combine(directory, "rules.json.tmp");
    }

    public async Task<RuleLoadResult> LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new RuleLoadResult([], Warning: null);
        }

        try
        {
            await using var stream = File.OpenRead(_filePath);
            var rules = await JsonSerializer.DeserializeAsync<List<Rule>>(stream) ?? [];
            return new RuleLoadResult(rules, Warning: null);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            TryBackupCorruptedFile();
            return new RuleLoadResult(
                [],
                Warning: "The saved rules file was corrupted and has been reset. A backup was saved as rules.json.bak.");
        }
    }

    public async Task SaveAsync(IReadOnlyList<Rule> rules)
    {
        try
        {
            await using (var stream = File.Create(_tempPath))
            {
                await JsonSerializer.SerializeAsync(stream, rules, new JsonSerializerOptions { WriteIndented = true });
            }

            File.Move(_tempPath, _filePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new RulePersistenceException("Could not save rules to disk.", ex);
        }
    }

    private void TryBackupCorruptedFile()
    {
        try
        {
            File.Copy(_filePath, _backupPath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Backing up the corrupted file is best-effort — losing it doesn't prevent recovery with an empty rule set.
        }
    }
}
