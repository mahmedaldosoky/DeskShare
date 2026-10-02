namespace DeskShare.Application.Bookings;

public sealed record SaveBookingRequest(Guid DeskId, DateOnly Date);
