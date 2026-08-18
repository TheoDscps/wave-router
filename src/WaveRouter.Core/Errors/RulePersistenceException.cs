namespace WaveRouter.Core.Errors;

/// <summary>Raised when rules can't be loaded from or saved to disk (see docs/use-cases/rule-persistence.md).</summary>
public sealed class RulePersistenceException : AppError
{
    public RulePersistenceException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
