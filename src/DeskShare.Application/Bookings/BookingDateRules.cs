using DeskShare.Application.Common.Exceptions;
using DeskShare.Domain.Bookings;

namespace DeskShare.Application.Bookings;

public static class BookingDateRules
{
    public static void EnsureDateIsNotInPast(DateOnly date, DateOnly today)
    {
        if (date < today)
            throw new BusinessRuleException("Bookings cannot be made for past dates.");
    }

    public static void EnsureBookingCanBeCancelled(Booking booking, DateOnly today)
    {
        if (booking.IsInPast(today))
            throw new BusinessRuleException("Past bookings cannot be cancelled.");
    }
}
