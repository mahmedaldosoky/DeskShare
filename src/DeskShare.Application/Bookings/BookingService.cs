using AutoMapper;
using DeskShare.Application.Abstractions;
using DeskShare.Application.Common;
using DeskShare.Application.Common.Exceptions;
using DeskShare.Domain.Bookings;
using DeskShare.Domain.Desks;
using DeskShare.Domain.Employees;

namespace DeskShare.Application.Bookings;

public sealed class BookingService(
    IBookingRepository bookings,
    IDeskRepository desks,
    IEmployeeRepository employees,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    TimeProvider timeProvider)
{
    public async Task<IReadOnlyList<BookingDto>> GetMyUpcomingAsync(CancellationToken cancellationToken)
    {
        var today = timeProvider.GetLocalToday();
        var myBookings = await bookings.GetForEmployeeFromAsync(currentUser.EmployeeId, today, cancellationToken);
        return mapper.Map<IReadOnlyList<BookingDto>>(myBookings);
    }

    public async Task<IReadOnlyList<BookingDto>> GetAllOnDateAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var bookingsOnDate = await bookings.GetOnDateAsync(date, cancellationToken);
        return mapper.Map<IReadOnlyList<BookingDto>>(bookingsOnDate);
    }

    public async Task<BookingDto> CreateAsync(SaveBookingRequest request, CancellationToken cancellationToken)
    {
        var desk = await GetExistingDeskAsync(request.DeskId, cancellationToken);
        var employee = await GetCurrentEmployeeAsync(cancellationToken);

        BookingDateRules.EnsureDateIsNotInPast(request.Date, timeProvider.GetLocalToday());

        var booking = Booking.Create(desk, employee, request.Date);
        await EnsureSlotIsFreeAsync(booking, cancellationToken);

        bookings.Add(booking);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<BookingDto>(booking);
    }

    public async Task CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var booking = await GetOwnedBookingAsync(id, cancellationToken);
        BookingDateRules.EnsureBookingCanBeCancelled(booking, timeProvider.GetLocalToday());

        bookings.Remove(booking);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureSlotIsFreeAsync(Booking booking, CancellationToken cancellationToken)
    {
        if (await bookings.IsDeskBookedAsync(booking.DeskId, booking.Date, cancellationToken))
            throw new ConflictException($"This desk is already booked on {booking.Date:yyyy-MM-dd}.");

        if (await bookings.HasEmployeeBookedAsync(booking.EmployeeId, booking.Date, cancellationToken))
            throw new ConflictException($"You already have a desk booked on {booking.Date:yyyy-MM-dd}.");
    }

    private async Task<Booking> GetOwnedBookingAsync(Guid id, CancellationToken cancellationToken)
    {
        var booking = await bookings.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Booking", id);

        if (!booking.IsOwnedBy(currentUser.EmployeeId))
            throw new ForbiddenException("You can only cancel your own bookings.");

        return booking;
    }

    private async Task<Desk> GetExistingDeskAsync(Guid id, CancellationToken cancellationToken) =>
        await desks.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Desk", id);

    private async Task<Employee> GetCurrentEmployeeAsync(CancellationToken cancellationToken) =>
        await employees.GetByIdAsync(currentUser.EmployeeId, cancellationToken)
            ?? throw new NotFoundException("Employee", currentUser.EmployeeId);
}
