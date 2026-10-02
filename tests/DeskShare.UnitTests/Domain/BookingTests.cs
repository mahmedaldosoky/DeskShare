using DeskShare.Domain;
using DeskShare.Domain.Bookings;
using DeskShare.Domain.Desks;
using DeskShare.Domain.Employees;

namespace DeskShare.UnitTests.Domain;

public sealed class BookingTests
{
    private static readonly DateOnly Today = new(2026, 10, 2);
    private readonly Desk _desk = Desk.Create("A-101", 1, DeskFeatures.None);
    private readonly Employee _employee = Employee.Create("sub-1", "sara@contoso.com", "Sara");

    [Fact]
    public void Create_AssignsDeskEmployeeAndDate()
    {
        var booking = Booking.Create(_desk, _employee, Today);

        Assert.Equal(_desk.Id, booking.DeskId);
        Assert.Equal(Today, booking.Date);
        Assert.True(booking.IsOwnedBy(_employee.Id));
    }

    [Fact]
    public void Reschedule_MovesDeskAndDate()
    {
        var booking = Booking.Create(_desk, _employee, Today);
        var otherDesk = Desk.Create("B-201", 2, DeskFeatures.Window);

        booking.Reschedule(otherDesk, Today.AddDays(3));

        Assert.Equal(otherDesk.Id, booking.DeskId);
        Assert.Equal(Today.AddDays(3), booking.Date);
    }

    [Fact]
    public void IsInPast_IsTrueOnlyAfterTheBookingDate()
    {
        var booking = Booking.Create(_desk, _employee, Today);

        Assert.False(booking.IsInPast(Today));
        Assert.True(booking.IsInPast(Today.AddDays(1)));
    }
}
