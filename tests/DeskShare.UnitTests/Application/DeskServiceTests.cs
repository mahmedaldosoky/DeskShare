using DeskShare.Application.Common.Exceptions;
using DeskShare.Application.Desks;
using DeskShare.Domain;
using DeskShare.Domain.Bookings;
using DeskShare.Domain.Desks;
using DeskShare.Domain.Employees;
using DeskShare.UnitTests.Fakes;

namespace DeskShare.UnitTests.Application;

public sealed class DeskServiceTests
{
    private static readonly DateOnly Today = new(2026, 10, 2);

    private readonly InMemoryStore _store = new();
    private readonly DeskService _service;

    public DeskServiceTests()
    {
        _service = new DeskService(
            new InMemoryDeskRepository(_store),
            new InMemoryBookingRepository(_store),
            new CountingUnitOfWork(),
            TestMapper.Instance,
            new FixedTimeProvider(Today));
    }

    [Fact]
    public async Task GetAvailable_RejectsPastDate()
    {
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.GetAvailableAsync(Today.AddDays(-1), CancellationToken.None));
    }

    [Fact]
    public async Task Create_StoresDeskWithCombinedFeatures()
    {
        var request = new SaveDeskRequest("a-101", 1, [DeskFeatures.Monitor, DeskFeatures.Window]);

        var desk = await _service.CreateAsync(request, CancellationToken.None);

        Assert.Equal("A-101", desk.Code);
        Assert.Equal([DeskFeatures.Monitor, DeskFeatures.Window], desk.Features);
        Assert.Equal(DeskFeatures.Monitor | DeskFeatures.Window, _store.Desks.Single().Features);
    }

    [Fact]
    public async Task Create_RejectsDuplicateCode()
    {
        _store.Desks.Add(Desk.Create("A-101", 1, DeskFeatures.None));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateAsync(new SaveDeskRequest("a-101", 2, null), CancellationToken.None));
    }

    [Fact]
    public async Task Update_AllowsKeepingTheSameCode()
    {
        var desk = Desk.Create("A-101", 1, DeskFeatures.None);
        _store.Desks.Add(desk);

        var updated = await _service.UpdateAsync(
            desk.Id, new SaveDeskRequest("A-101", 4, [DeskFeatures.Standing]), CancellationToken.None);

        Assert.Equal(4, updated.Floor);
    }

    [Fact]
    public async Task Delete_RejectsDeskWithUpcomingBookings()
    {
        var desk = AddDeskBookedOn(Today.AddDays(2));

        await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync(desk.Id, CancellationToken.None));
        Assert.Single(_store.Desks);
    }

    [Fact]
    public async Task Delete_SoftDeletesDeskSoPastBookingsKeepIt()
    {
        var desk = Desk.Create("A-101", 1, DeskFeatures.None);
        _store.Desks.Add(desk);

        await _service.DeleteAsync(desk.Id, CancellationToken.None);

        Assert.True(desk.IsDeleted);
        Assert.Single(_store.Desks);
        Assert.Empty(await _service.GetAllAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Create_AllowsReusingCodeOfDeletedDesk()
    {
        var deleted = Desk.Create("A-101", 1, DeskFeatures.None);
        deleted.Delete();
        _store.Desks.Add(deleted);

        var desk = await _service.CreateAsync(new SaveDeskRequest("A-101", 1, null), CancellationToken.None);

        Assert.NotEqual(deleted.Id, desk.Id);
    }

    [Fact]
    public async Task Delete_ThrowsWhenDeskDoesNotExist()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(Guid.NewGuid(), CancellationToken.None));
    }

    private Desk AddDeskBookedOn(DateOnly date)
    {
        var desk = Desk.Create("A-101", 1, DeskFeatures.None);
        var employee = Employee.Create("sub-1", "sara@contoso.com", "Sara");
        _store.Desks.Add(desk);
        _store.Bookings.Add(Booking.Create(desk, employee, date));
        return desk;
    }
}
