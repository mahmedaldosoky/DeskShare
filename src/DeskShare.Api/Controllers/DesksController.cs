using DeskShare.Api.Authentication;
using DeskShare.Application.Desks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DeskShare.Api.Controllers;

[ApiController]
[Route("api/desks")]
[Authorize]
public sealed class DesksController(DeskService deskService) : ControllerBase
{
    [HttpGet("available")]
    public Task<IReadOnlyList<DeskDto>> GetAvailable([FromQuery, BindRequired] DateOnly date, CancellationToken cancellationToken) =>
        deskService.GetAvailableAsync(date, cancellationToken);

    [HttpGet]
    [Authorize(Roles = Roles.OfficeManager)]
    public Task<IReadOnlyList<DeskDto>> GetAll(CancellationToken cancellationToken) =>
        deskService.GetAllAsync(cancellationToken);

    [HttpGet("{id:guid}")]
    [Authorize(Roles = Roles.OfficeManager)]
    public Task<DeskDto> GetById(Guid id, CancellationToken cancellationToken) =>
        deskService.GetByIdAsync(id, cancellationToken);

    [HttpPost]
    [Authorize(Roles = Roles.OfficeManager)]
    public async Task<ActionResult<DeskDto>> Create(SaveDeskRequest request, CancellationToken cancellationToken)
    {
        var desk = await deskService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = desk.Id }, desk);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.OfficeManager)]
    public Task<DeskDto> Update(Guid id, SaveDeskRequest request, CancellationToken cancellationToken) =>
        deskService.UpdateAsync(id, request, cancellationToken);

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.OfficeManager)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deskService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
