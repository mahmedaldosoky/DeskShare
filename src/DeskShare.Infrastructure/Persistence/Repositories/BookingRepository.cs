using DeskShare.Application.Abstractions;
using DeskShare.Domain.Bookings;
using Microsoft.EntityFrameworkCore;

namespace DeskShare.Infrastructure.Persistence.Repositories;

internal sealed class BookingRepository(DeskShareDbContext dbContext) : IBookingRepository
{
    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        BookingsWithDetails().FirstOrDefaultAsync(booking => booking.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Booking>> GetForEmployeeFromAsync(
        Guid employeeId, DateOnly fromDate, CancellationToken cancellationToken) =>
        await BookingsWithDetails()
            .AsNoTracking()
            .Where(booking => booking.EmployeeId == employeeId && booking.Date >= fromDate)
            .OrderBy(booking => booking.Date)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Booking>> GetOnDateAsync(DateOnly date, CancellationToken cancellationToken) =>
        await BookingsWithDetails()
            .AsNoTracking()
            .Where(booking => booking.Date == date)
            .OrderBy(booking => booking.Desk.Floor)
            .ThenBy(booking => booking.Desk.Code)
            .ToListAsync(cancellationToken);

    public Task<bool> IsDeskBookedAsync(
        Guid deskId, DateOnly date, Guid? excludingBookingId, CancellationToken cancellationToken) =>
        dbContext.Bookings.AnyAsync(
            booking => booking.DeskId == deskId && booking.Date == date && booking.Id != excludingBookingId,
            cancellationToken);

    public Task<bool> HasEmployeeBookedAsync(
        Guid employeeId, DateOnly date, Guid? excludingBookingId, CancellationToken cancellationToken) =>
        dbContext.Bookings.AnyAsync(
            booking => booking.EmployeeId == employeeId && booking.Date == date && booking.Id != excludingBookingId,
            cancellationToken);

    public Task<bool> DeskHasBookingsFromAsync(Guid deskId, DateOnly fromDate, CancellationToken cancellationToken) =>
        dbContext.Bookings.AnyAsync(
            booking => booking.DeskId == deskId && booking.Date >= fromDate,
            cancellationToken);

    public void Add(Booking booking) => dbContext.Bookings.Add(booking);

    public void Remove(Booking booking) => dbContext.Bookings.Remove(booking);

    private IQueryable<Booking> BookingsWithDetails() =>
        dbContext.Bookings
            .Include(booking => booking.Desk)
            .Include(booking => booking.Employee);
}
