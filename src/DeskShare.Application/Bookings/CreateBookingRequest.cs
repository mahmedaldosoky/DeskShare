namespace DeskShare.Application.Bookings;

public sealed record CreateBookingRequest(Guid DeskId, DateOnly Date);
