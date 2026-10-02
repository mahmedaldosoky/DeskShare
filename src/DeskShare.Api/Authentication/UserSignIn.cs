using System.Security.Claims;
using DeskShare.Application.Employees;

namespace DeskShare.Api.Authentication;

// Turns a signed-in person (from SSO or the development form) into the user stored in the DeskShare cookie.
public sealed class UserSignIn(EmployeeProvisioningService employeeProvisioning)
{
    public async Task<ClaimsPrincipal> CreatePrincipalAsync(
        ExternalIdentity identity,
        bool isOfficeManager,
        CancellationToken cancellationToken)
    {
        var employeeId = await employeeProvisioning.ProvisionAsync(identity, cancellationToken);

        var claims = new List<Claim>
        {
            new(CurrentUser.EmployeeIdClaim, employeeId.ToString()),
            new(ClaimTypes.Name, identity.DisplayName),
            new(ClaimTypes.Role, Roles.Employee),
        };

        if (isOfficeManager)
            claims.Add(new Claim(ClaimTypes.Role, Roles.OfficeManager));

        var claimsIdentity = new ClaimsIdentity(claims, "DeskShare", ClaimTypes.Name, ClaimTypes.Role);
        return new ClaimsPrincipal(claimsIdentity);
    }
}
