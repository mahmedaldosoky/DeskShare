using DeskShare.Application.Abstractions;
using DeskShare.Domain.Bookings;
using DeskShare.Domain.Desks;
using DeskShare.Domain.Employees;

namespace DeskShare.UnitTests.Fakes;

internal sealed class InMemoryStore
{
    public List<Desk> Desks { get; } = [];
    public List<Booking> Bookings { get; } = [];
    public List<Employee> Employees { get; } = [];
}

internal sealed class InMemoryDeskRepository(InMemoryStore store) : IDeskRepository
{
    public Task<IReadOnlyList<Desk>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Desk>>(ActiveDesks().ToList());

    public Task<IReadOnlyList<Desk>> GetAvailableOnAsync(DateOnly date, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Desk>>(ActiveDesks()
            .Where(desk => !store.Bookings.Any(booking => booking.DeskId == desk.Id && booking.Date == date))
            .ToList());

    public Task<Desk?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(ActiveDesks().FirstOrDefault(desk => desk.Id == id));

    public Task<bool> CodeExistsAsync(string code, Guid? excludingDeskId, CancellationToken cancellationToken) =>
        Task.FromResult(ActiveDesks().Any(desk => desk.Code == code && desk.Id != excludingDeskId));

    public void Add(Desk desk) => store.Desks.Add(desk);

    private IEnumerable<Desk> ActiveDesks() => store.Desks.Where(desk => !desk.IsDeleted);
}

internal sealed class InMemoryBookingRepository(InMemoryStore store) : IBookingRepository
{
    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(store.Bookings.FirstOrDefault(booking => booking.Id == id));

    public Task<IReadOnlyList<Booking>> GetForEmployeeFromAsync(
        Guid employeeId, DateOnly fromDate, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Booking>>(store.Bookings
            .Where(booking => booking.EmployeeId == employeeId && booking.Date >= fromDate)
            .ToList());

    public Task<IReadOnlyList<Booking>> GetOnDateAsync(DateOnly date, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Booking>>(store.Bookings.Where(booking => booking.Date == date).ToList());

    public Task<bool> IsDeskBookedAsync(
        Guid deskId, DateOnly date, CancellationToken cancellationToken) =>
        Task.FromResult(store.Bookings.Any(booking =>
            booking.DeskId == deskId && booking.Date == date));

    public Task<bool> HasEmployeeBookedAsync(
        Guid employeeId, DateOnly date, CancellationToken cancellationToken) =>
        Task.FromResult(store.Bookings.Any(booking =>
            booking.EmployeeId == employeeId && booking.Date == date));

    public Task<bool> DeskHasBookingsFromAsync(Guid deskId, DateOnly fromDate, CancellationToken cancellationToken) =>
        Task.FromResult(store.Bookings.Any(booking => booking.DeskId == deskId && booking.Date >= fromDate));

    public void Add(Booking booking) => store.Bookings.Add(booking);

    public void Remove(Booking booking) => store.Bookings.Remove(booking);
}

internal sealed class InMemoryEmployeeRepository(InMemoryStore store) : IEmployeeRepository
{
    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(store.Employees.FirstOrDefault(employee => employee.Id == id));

    public Task<Employee?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken) =>
        Task.FromResult(store.Employees.FirstOrDefault(employee => employee.ExternalId == externalId));

    public void Add(Employee employee) => store.Employees.Add(employee);
}

internal sealed class CountingUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}

internal sealed class FakeCurrentUser : ICurrentUser
{
    public Guid EmployeeId { get; set; }

    public Guid? FindEmployeeId() => EmployeeId;
}
