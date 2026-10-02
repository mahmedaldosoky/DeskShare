using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DeskShare.Application.Abstractions;
using DeskShare.Application.Bookings;
using DeskShare.Application.Common.Exceptions;
using DeskShare.Application.Desks;
using DeskShare.Infrastructure.Persistence;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DeskShare.Api.IntegrationTests;

public sealed class DeskShareApiTests(DeskShareApiFactory factory) : IClassFixture<DeskShareApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private static readonly DateOnly NextWeek = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

    [Fact]
    public async Task AnonymousUser_IsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/bookings/mine");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Employee_CannotManageDesks()
    {
        var employee = await factory.CreateSignedInClientAsync("Nour");

        var response = await employee.PostAsJsonAsync("/api/desks", new { Code = "X-1", Floor = 1 });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task InvalidDesk_ListsEveryInvalidField()
    {
        var manager = await factory.CreateSignedInClientAsync("Omar", isOfficeManager: true);

        var response = await manager.PostAsJsonAsync("/api/desks", new { Code = "", Floor = 999 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        var invalidFields = problem.GetProperty("errors").EnumerateObject().Select(field => field.Name);
        Assert.Equal(["Code", "Floor"], invalidFields.Order());
    }

    [Fact]
    public async Task DeskWithUnknownFeature_IsRejected()
    {
        var manager = await factory.CreateSignedInClientAsync("Omar", isOfficeManager: true);

        var response = await manager.PostAsJsonAsync("/api/desks", new { Code = "A-1", Floor = 1, Features = new[] { 64 } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RepeatedUnexpectedError_IsStoredOnceInOperationalLogs()
    {
        using var failingApi = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddScoped<IEmployeeRepository, FailingEmployeeRepository>()));
        var client = failingApi.CreateClient();
        var signIn = new { DisplayName = "Laila", Email = "laila@contoso.com", IsOfficeManager = false };

        var responses = new List<HttpResponseMessage> { await client.PostAsJsonAsync("/api/auth/dev-login", signIn) };
        responses.AddRange(await Task.WhenAll(
            Enumerable.Range(0, 5).Select(_ => client.PostAsJsonAsync("/api/auth/dev-login", signIn))));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode));
        await using var scope = failingApi.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DeskShareDbContext>();
        var log = await dbContext.OperationalLogs.SingleAsync(log => log.Source.Contains(nameof(FailingEmployeeRepository)));
        Assert.Equal(6, log.OccurrenceCount);
        Assert.Equal(typeof(InvalidOperationException).FullName, log.ExceptionType);
        Assert.Equal("/api/auth/dev-login", log.RequestPath);
    }

    [Fact]
    public async Task ExpectedBusinessError_IsNotStoredInOperationalLogs()
    {
        var manager = await factory.CreateSignedInClientAsync("Omar", isOfficeManager: true);

        var response = await manager.GetAsync($"/api/desks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DeskShareDbContext>();
        Assert.False(await dbContext.OperationalLogs.AnyAsync(log => log.ExceptionType == typeof(NotFoundException).FullName));
    }

    [Fact]
    public async Task DeskWithUpcomingBooking_CannotBeDeletedUntilBookingIsCancelled()
    {
        var manager = await factory.CreateSignedInClientAsync("Omar", isOfficeManager: true);
        var employee = await factory.CreateSignedInClientAsync("Sara");

        var desk = await CreateDeskAsync(manager, "T-100");
        var booking = await BookAsync(employee, desk.Id, NextWeek);

        var blockedDelete = await manager.DeleteAsync($"/api/desks/{desk.Id}");
        Assert.Equal(HttpStatusCode.Conflict, blockedDelete.StatusCode);

        var cancel = await employee.DeleteAsync($"/api/bookings/{booking.Id}");
        Assert.Equal(HttpStatusCode.NoContent, cancel.StatusCode);

        var delete = await manager.DeleteAsync($"/api/desks/{desk.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }

    [Fact]
    public async Task SameDeskOnSameDay_CanOnlyBeBookedOnce()
    {
        var manager = await factory.CreateSignedInClientAsync("Mona", isOfficeManager: true);
        var first = await factory.CreateSignedInClientAsync("Ali");
        var second = await factory.CreateSignedInClientAsync("Hana");
        var desk = await CreateDeskAsync(manager, "T-200");

        await BookAsync(first, desk.Id, NextWeek);
        var response = await second.PostAsJsonAsync("/api/bookings", new SaveBookingRequest(desk.Id, NextWeek));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task BookedDesk_IsNotListedAsAvailable()
    {
        var manager = await factory.CreateSignedInClientAsync("Karim", isOfficeManager: true);
        var employee = await factory.CreateSignedInClientAsync("Laila");
        var desk = await CreateDeskAsync(manager, "T-300");
        var date = NextWeek.AddDays(1);

        await BookAsync(employee, desk.Id, date);
        var available = await employee.GetFromJsonAsync<List<DeskDto>>(
            $"/api/desks/available?date={date:yyyy-MM-dd}", JsonOptions);

        Assert.DoesNotContain(available!, availableDesk => availableDesk.Id == desk.Id);
    }

    [Fact]
    public async Task DeletedDesk_IsHiddenAndFreesItsCode()
    {
        var manager = await factory.CreateSignedInClientAsync("Salma", isOfficeManager: true);
        var desk = await CreateDeskAsync(manager, "T-500");

        var delete = await manager.DeleteAsync($"/api/desks/{desk.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var lookup = await manager.GetAsync($"/api/desks/{desk.Id}");
        Assert.Equal(HttpStatusCode.NotFound, lookup.StatusCode);

        var recreated = await CreateDeskAsync(manager, "T-500");
        Assert.NotEqual(desk.Id, recreated.Id);
    }

    [Fact]
    public async Task AvailableDesks_RejectsPastDate()
    {
        var employee = await factory.CreateSignedInClientAsync("Rana");
        var yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

        var response = await employee.GetAsync($"/api/desks/available?date={yesterday:yyyy-MM-dd}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Employee_CannotCancelSomeoneElsesBooking()
    {
        var manager = await factory.CreateSignedInClientAsync("Youssef", isOfficeManager: true);
        var owner = await factory.CreateSignedInClientAsync("Dina");
        var other = await factory.CreateSignedInClientAsync("Tarek");
        var desk = await CreateDeskAsync(manager, "T-400");
        var booking = await BookAsync(owner, desk.Id, NextWeek);

        var response = await other.DeleteAsync($"/api/bookings/{booking.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<DeskDto> CreateDeskAsync(HttpClient manager, string code)
    {
        var response = await manager.PostAsJsonAsync("/api/desks", new { Code = code, Floor = 1, Features = new[] { "Monitor" } });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<DeskDto>(JsonOptions))!;
    }

    private static async Task<BookingDto> BookAsync(HttpClient employee, Guid deskId, DateOnly date)
    {
        var response = await employee.PostAsJsonAsync("/api/bookings", new SaveBookingRequest(deskId, date));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<BookingDto>(JsonOptions))!;
    }
}
