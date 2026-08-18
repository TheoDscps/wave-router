namespace WaveRouter.Core.Errors;

public abstract class AppError : Exception
{
    protected AppError(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
