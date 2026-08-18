namespace WaveRouter.Core.Models;

public sealed record RoutingResult(bool Success, string? ErrorMessage)
{
    public static RoutingResult Ok() => new(true, null);
    public static RoutingResult Failed(string errorMessage) => new(false, errorMessage);
}
