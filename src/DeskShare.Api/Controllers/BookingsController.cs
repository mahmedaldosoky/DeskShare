using DeskShare.Api.Authentication;
using DeskShare.Application.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DeskShare.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public sealed class BookingsController(BookingService bookingService) : ControllerBase
{
    [HttpGet("mine")]
    public Task<IReadOnlyList<BookingDto>> GetMine(CancellationToken cancellationToken) =>
        bookingService.GetMyUpcomingAsync(cancellationToken);

    [HttpGet]
    [Authorize(Roles = Roles.OfficeManager)]
    public Task<IReadOnlyList<BookingDto>> GetAllOnDate([FromQuery, BindRequired] DateOnly date, CancellationToken cancellationToken) =>
        bookingService.GetAllOnDateAsync(date, cancellationToken);

    [HttpPost]
    public async Task<IActionResult> Create(SaveBookingRequest request, CancellationToken cancellationToken)
    {
        var booking = await bookingService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, booking);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await bookingService.CancelAsync(id, cancellationToken);
        return NoContent();
    }
}
