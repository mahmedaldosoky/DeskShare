using DeskShare.Domain.Desks;

namespace DeskShare.Application.Abstractions;

public interface IDeskRepository
{
    Task<IReadOnlyList<Desk>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Desk>> GetAvailableOnAsync(DateOnly date, CancellationToken cancellationToken);
    Task<Desk?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> CodeExistsAsync(string code, Guid? excludingDeskId, CancellationToken cancellationToken);
    void Add(Desk desk);
}
