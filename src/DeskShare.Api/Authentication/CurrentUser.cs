using System.Security.Claims;
using DeskShare.Application.Abstractions;

namespace DeskShare.Api.Authentication;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid EmployeeId =>
        FindEmployeeId() ?? throw new InvalidOperationException("There is no signed-in employee for this request.");

    public Guid? FindEmployeeId() =>
        Guid.TryParse(httpContextAccessor.HttpContext?.User.FindFirstValue(DeskShareClaimTypes.EmployeeId), out var id)
            ? id
            : null;
}
