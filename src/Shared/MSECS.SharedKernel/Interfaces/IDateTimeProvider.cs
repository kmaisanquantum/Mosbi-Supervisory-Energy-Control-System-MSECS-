namespace MSECS.SharedKernel.Interfaces;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
