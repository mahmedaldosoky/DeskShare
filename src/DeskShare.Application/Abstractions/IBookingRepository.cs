using DeskShare.Domain.Bookings;

namespace DeskShare.Application.Abstractions;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Booking>> GetForEmployeeFromAsync(Guid employeeId, DateOnly fromDate, CancellationToken cancellationToken);
    Task<IReadOnlyList<Booking>> GetOnDateAsync(DateOnly date, CancellationToken cancellationToken);
    Task<bool> IsDeskBookedAsync(Guid deskId, DateOnly date, CancellationToken cancellationToken);
    Task<bool> HasEmployeeBookedAsync(Guid employeeId, DateOnly date, CancellationToken cancellationToken);
    Task<bool> DeskHasBookingsFromAsync(Guid deskId, DateOnly fromDate, CancellationToken cancellationToken);
    void Add(Booking booking);
    void Remove(Booking booking);
}
