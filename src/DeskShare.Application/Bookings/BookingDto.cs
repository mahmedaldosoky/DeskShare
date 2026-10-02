namespace DeskShare.Application.Bookings;

public sealed record BookingDto(
    Guid Id,
    DateOnly Date,
    Guid DeskId,
    string DeskCode,
    int DeskFloor,
    string EmployeeName);
