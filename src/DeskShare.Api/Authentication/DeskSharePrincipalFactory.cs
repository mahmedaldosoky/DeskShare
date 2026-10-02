using System.Security.Claims;
using DeskShare.Application.Employees;
using Microsoft.Extensions.Options;

namespace DeskShare.Api.Authentication;

/// <summary>
/// Turns an identity coming from any sign-in method into the principal stored in the DeskShare cookie.
/// </summary>
public sealed class DeskSharePrincipalFactory(
    EmployeeProvisioningService employeeProvisioning,
    IOptions<DeskShareAuthenticationOptions> options)
{
    private const string AuthenticationType = "DeskShare";

    public async Task<ClaimsPrincipal> CreateAsync(
        ExternalIdentity identity,
        IEnumerable<string> groups,
        CancellationToken cancellationToken)
    {
        var employeeId = await employeeProvisioning.ProvisionAsync(identity, cancellationToken);

        var claims = new List<Claim>
        {
            new(DeskShareClaimTypes.EmployeeId, employeeId.ToString()),
            new(ClaimTypes.Name, identity.DisplayName),
            new(ClaimTypes.Role, Roles.Employee),
        };

        if (IsOfficeManager(groups))
            claims.Add(new Claim(ClaimTypes.Role, Roles.OfficeManager));

        var claimsIdentity = new ClaimsIdentity(claims, AuthenticationType, ClaimTypes.Name, ClaimTypes.Role);
        return new ClaimsPrincipal(claimsIdentity);
    }

    private bool IsOfficeManager(IEnumerable<string> groups) =>
        groups.Intersect(options.Value.OfficeManagerGroups, StringComparer.OrdinalIgnoreCase).Any();
}
