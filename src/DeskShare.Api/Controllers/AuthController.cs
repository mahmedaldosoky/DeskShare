using System.Security.Claims;
using DeskShare.Api.Authentication;
using DeskShare.Api.Contracts;
using DeskShare.Application.Employees;
using DeskShare.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DeskShare.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController(IOptions<DeskShareAuthenticationOptions> options) : ControllerBase
{
    private SignInMode Mode => options.Value.Mode;

    [HttpGet("session")]
    public SessionResponse GetSession()
    {
        if (User.Identity?.IsAuthenticated != true)
            return new SessionResponse(Mode, User: null);

        var sessionUser = new SessionUser(
            User.Identity.Name ?? string.Empty,
            User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToList());

        return new SessionResponse(Mode, sessionUser);
    }

    [HttpGet("login")]
    public IActionResult Login([FromQuery] string? returnUrl)
    {
        if (Mode != SignInMode.Oidc)
            return NotFound();

        var properties = new AuthenticationProperties { RedirectUri = ToSafeLocalUrl(returnUrl) };
        return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpPost("dev-login")]
    public async Task<IActionResult> DevelopmentLogin(
        DevelopmentSignInRequest request,
        [FromServices] DeskSharePrincipalFactory principalFactory,
        CancellationToken cancellationToken)
    {
        if (Mode != SignInMode.Development)
            return NotFound();

        var identity = new ExternalIdentity($"dev:{request.Email.ToLowerInvariant()}", request.Email, request.DisplayName);
        var groups = request.IsOfficeManager ? options.Value.OfficeManagerGroups : [];
        var principal = await principalFactory.CreateAsync(identity, groups, cancellationToken);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        return NoContent();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (Mode != SignInMode.Oidc)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect("/");
        }

        var properties = new AuthenticationProperties { RedirectUri = "/" };
        return SignOut(properties, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
    }

    private string ToSafeLocalUrl(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
}
