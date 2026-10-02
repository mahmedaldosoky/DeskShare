using DeskShare.Domain.OperationalLogs;

namespace DeskShare.Application.Abstractions;

public interface IOperationalLogRepository
{
    Task AddOrIncrementAsync(OperationalLog log, CancellationToken cancellationToken);
}
