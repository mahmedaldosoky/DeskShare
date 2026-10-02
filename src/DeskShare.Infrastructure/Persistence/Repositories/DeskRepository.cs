using DeskShare.Application.Abstractions;
using DeskShare.Domain.Desks;
using Microsoft.EntityFrameworkCore;

namespace DeskShare.Infrastructure.Persistence.Repositories;

internal sealed class DeskRepository(DeskShareDbContext dbContext) : IDeskRepository
{
    public async Task<IReadOnlyList<Desk>> GetAllAsync(CancellationToken cancellationToken) =>
        await OrderedDesks().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Desk>> GetAvailableOnAsync(DateOnly date, CancellationToken cancellationToken) =>
        await OrderedDesks()
            .Where(desk => !dbContext.Bookings.Any(booking => booking.DeskId == desk.Id && booking.Date == date))
            .ToListAsync(cancellationToken);

    public Task<Desk?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        ActiveDesks().FirstOrDefaultAsync(desk => desk.Id == id, cancellationToken);

    public Task<bool> CodeExistsAsync(string code, Guid? excludingDeskId, CancellationToken cancellationToken) =>
        ActiveDesks().AnyAsync(desk => desk.Code == code && desk.Id != excludingDeskId, cancellationToken);

    public void Add(Desk desk) => dbContext.Desks.Add(desk);

    private IQueryable<Desk> ActiveDesks() => dbContext.Desks.Where(desk => !desk.IsDeleted);

    private IQueryable<Desk> OrderedDesks() =>
        ActiveDesks().AsNoTracking().OrderBy(desk => desk.Floor).ThenBy(desk => desk.Code);
}
