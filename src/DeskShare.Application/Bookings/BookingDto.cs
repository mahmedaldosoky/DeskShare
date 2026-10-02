namespace DeskShare.Application.Bookings;

public sealed record BookingDto(
    Guid Id,
    DateOnly Date,
    string DeskCode,
    int DeskFloor,
    string EmployeeName);
