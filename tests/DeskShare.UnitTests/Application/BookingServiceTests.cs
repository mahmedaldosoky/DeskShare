using DeskShare.Application.Bookings;
using DeskShare.Application.Common.Exceptions;
using DeskShare.Domain;
using DeskShare.Domain.Bookings;
using DeskShare.Domain.Desks;
using DeskShare.Domain.Employees;
using DeskShare.UnitTests.Fakes;

namespace DeskShare.UnitTests.Application;

public sealed class BookingServiceTests
{
    private static readonly DateOnly Today = new(2026, 10, 2);
    private static readonly DateOnly Tomorrow = Today.AddDays(1);

    private readonly InMemoryStore _store = new();
    private readonly CountingUnitOfWork _unitOfWork = new();
    private readonly FakeCurrentUser _currentUser = new();
    private readonly BookingService _service;

    private readonly Desk _deskA = Desk.Create("A-101", 1, DeskFeatures.Monitor);
    private readonly Desk _deskB = Desk.Create("B-201", 2, DeskFeatures.Window);
    private readonly Employee _sara = Employee.Create("sub-sara", "sara@contoso.com", "Sara");
    private readonly Employee _omar = Employee.Create("sub-omar", "omar@contoso.com", "Omar");

    public BookingServiceTests()
    {
        _store.Desks.AddRange([_deskA, _deskB]);
        _store.Employees.AddRange([_sara, _omar]);
        _currentUser.EmployeeId = _sara.Id;

        _service = new BookingService(
            new InMemoryBookingRepository(_store),
            new InMemoryDeskRepository(_store),
            new InMemoryEmployeeRepository(_store),
            _currentUser,
            _unitOfWork,
            TestMapper.Instance,
            new FixedTimeProvider(Today));
    }

    [Fact]
    public async Task Create_BooksFreeDeskForCurrentEmployee()
    {
        var booking = await _service.CreateAsync(new SaveBookingRequest(_deskA.Id, Tomorrow), CancellationToken.None);

        Assert.Equal(_sara.Id, _store.Bookings.Single().EmployeeId);
        Assert.Equal("A-101", booking.DeskCode);
        Assert.Single(_store.Bookings);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Create_RejectsDeskAlreadyBookedThatDay()
    {
        AddBooking(_deskA, _omar, Tomorrow);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateAsync(new SaveBookingRequest(_deskA.Id, Tomorrow), CancellationToken.None));
    }

    [Fact]
    public async Task Create_RejectsSecondDeskForSameEmployeeAndDay()
    {
        AddBooking(_deskA, _sara, Tomorrow);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateAsync(new SaveBookingRequest(_deskB.Id, Tomorrow), CancellationToken.None));
    }

    [Fact]
    public async Task Create_RejectsPastDate()
    {
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.CreateAsync(new SaveBookingRequest(_deskA.Id, Today.AddDays(-1)), CancellationToken.None));
        Assert.Empty(_store.Bookings);
    }

    [Fact]
    public async Task Create_RejectsUnknownDesk()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreateAsync(new SaveBookingRequest(Guid.NewGuid(), Tomorrow), CancellationToken.None));
    }

    [Fact]
    public async Task Update_AllowsKeepingTheSameDeskAndDate()
    {
        var booking = AddBooking(_deskA, _sara, Tomorrow);

        var updated = await _service.UpdateAsync(
            booking.Id, new SaveBookingRequest(_deskA.Id, Tomorrow), CancellationToken.None);

        Assert.Equal(booking.Id, updated.Id);
    }

    [Fact]
    public async Task Update_MovesBookingToAnotherDesk()
    {
        var booking = AddBooking(_deskA, _sara, Tomorrow);

        var updated = await _service.UpdateAsync(
            booking.Id, new SaveBookingRequest(_deskB.Id, Tomorrow), CancellationToken.None);

        Assert.Equal(_deskB.Id, updated.DeskId);
    }

    [Fact]
    public async Task Update_RejectsSomeoneElsesBooking()
    {
        var omarsBooking = AddBooking(_deskA, _omar, Tomorrow);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.UpdateAsync(omarsBooking.Id, new SaveBookingRequest(_deskB.Id, Tomorrow), CancellationToken.None));
    }

    [Fact]
    public async Task Update_RejectsBookingThatAlreadyHappened()
    {
        var pastBooking = AddBooking(_deskA, _sara, Today.AddDays(-1));

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.UpdateAsync(pastBooking.Id, new SaveBookingRequest(_deskA.Id, Tomorrow), CancellationToken.None));
    }

    [Fact]
    public async Task Update_RejectsMovingToPastDate()
    {
        var booking = AddBooking(_deskA, _sara, Tomorrow);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.UpdateAsync(booking.Id, new SaveBookingRequest(_deskA.Id, Today.AddDays(-1)), CancellationToken.None));
    }

    [Fact]
    public async Task Cancel_RejectsPastBooking()
    {
        var pastBooking = AddBooking(_deskA, _sara, Today.AddDays(-1));

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.CancelAsync(pastBooking.Id, CancellationToken.None));
        Assert.Single(_store.Bookings);
    }

    [Fact]
    public async Task Cancel_RemovesOwnBooking()
    {
        var booking = AddBooking(_deskA, _sara, Tomorrow);

        await _service.CancelAsync(booking.Id, CancellationToken.None);

        Assert.Empty(_store.Bookings);
    }

    [Fact]
    public async Task Cancel_RejectsSomeoneElsesBooking()
    {
        var omarsBooking = AddBooking(_deskA, _omar, Tomorrow);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.CancelAsync(omarsBooking.Id, CancellationToken.None));
        Assert.Single(_store.Bookings);
    }

    private Booking AddBooking(Desk desk, Employee employee, DateOnly date)
    {
        var booking = Booking.Create(desk, employee, date);
        _store.Bookings.Add(booking);
        return booking;
    }
}
