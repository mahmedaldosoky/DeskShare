using DeskShare.Domain.OperationalLogs;

namespace DeskShare.Application.Abstractions;

public interface IOperationalLogRepository
{
    /// <summary>Inserts the log, or increments the existing row with the same fingerprint.</summary>
    Task AddOrIncrementAsync(OperationalLog log, CancellationToken cancellationToken);
}
