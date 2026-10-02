using DeskShare.Application.Bookings;
using DeskShare.Application.Desks;
using DeskShare.Domain;
using DeskShare.Domain.Bookings;
using DeskShare.Domain.Desks;
using DeskShare.Domain.Employees;
using DeskShare.UnitTests.Fakes;

namespace DeskShare.UnitTests.Application;

public sealed class MappingProfileTests
{
    [Fact]
    public void Configuration_IsValid() => TestMapper.Configuration.AssertConfigurationIsValid();

    [Fact]
    public void Desk_MapsFeatureFlagsToList()
    {
        var desk = Desk.Create("A-101", 1, DeskFeatures.Monitor | DeskFeatures.Window);

        var dto = TestMapper.Instance.Map<DeskDto>(desk);

        Assert.Equal([DeskFeatures.Monitor, DeskFeatures.Window], dto.Features);
    }

    [Fact]
    public void Booking_FlattensDeskAndEmployee()
    {
        var today = new DateOnly(2026, 10, 2);
        var desk = Desk.Create("B-201", 2, DeskFeatures.None);
        var employee = Employee.Create("sub-sara", "sara@contoso.com", "Sara Ali");
        var booking = Booking.Create(desk, employee, today);

        var dto = TestMapper.Instance.Map<BookingDto>(booking);

        Assert.Equal(new BookingDto(booking.Id, today, "B-201", 2, "Sara Ali"), dto);
    }
}
