using System.Text.Json;
using WaveRouter.Core.Models;

namespace WaveRouter.Core.Tests.Models;

public class AppSettingsTests
{
    [Fact]
    public void Deserializing_SettingsJsonWrittenBeforeShowRoutingNotificationsExisted_DefaultsToTrue()
    {
        // Exactly what settings.json looked like before this field was added — a real user's existing
        // file on disk, so this has to keep working.
        const string legacyJson = """{"Theme":"Dark","Language":"fr"}""";

        var settings = JsonSerializer.Deserialize<AppSettings>(legacyJson);

        Assert.NotNull(settings);
        Assert.True(settings.ShowRoutingNotifications);
        Assert.Equal("Dark", settings.Theme);
        Assert.Equal("fr", settings.Language);
    }

    [Fact]
    public void Deserializing_SettingsJsonWithExplicitFalse_RespectsIt()
    {
        const string json = """{"Theme":"Light","Language":"en","ShowRoutingNotifications":false}""";

        var settings = JsonSerializer.Deserialize<AppSettings>(json);

        Assert.NotNull(settings);
        Assert.False(settings.ShowRoutingNotifications);
    }
}
