using System.Security.Claims;
using DeskShare.Application.Employees;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

namespace DeskShare.Api.Authentication;

public sealed class DeskShareOidcEvents(
    DeskSharePrincipalFactory principalFactory,
    IOptions<DeskShareAuthenticationOptions> options) : OpenIdConnectEvents
{
    private readonly OidcClaimTypes _claimTypes = options.Value.Oidc.ClaimTypes;

    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var identityProviderPrincipal = context.Principal!;

        context.Principal = await principalFactory.CreateAsync(
            ReadIdentity(identityProviderPrincipal),
            identityProviderPrincipal.FindAll(_claimTypes.Groups).Select(claim => claim.Value),
            context.HttpContext.RequestAborted);
    }

    // Tokens are not saved, so there is no id_token_hint; the client id tells the provider
    // which application's allowed logout URLs to check post_logout_redirect_uri against.
    public override Task RedirectToIdentityProviderForSignOut(RedirectContext context)
    {
        context.ProtocolMessage.ClientId = context.Options.ClientId;
        return Task.CompletedTask;
    }

    private ExternalIdentity ReadIdentity(ClaimsPrincipal principal)
    {
        var subject = principal.FindFirstValue(_claimTypes.Subject)
            ?? throw new InvalidOperationException($"The identity provider did not send a '{_claimTypes.Subject}' claim.");
        var email = principal.FindFirstValue(_claimTypes.Email)
            ?? throw new InvalidOperationException($"The identity provider did not send a '{_claimTypes.Email}' claim.");
        var displayName = principal.FindFirstValue(_claimTypes.Name) ?? email;

        return new ExternalIdentity(subject, email, displayName);
    }
}
