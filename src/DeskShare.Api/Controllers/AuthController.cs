using System.Security.Claims;
using DeskShare.Api.Authentication;
using DeskShare.Api.Contracts;
using DeskShare.Application.Employees;
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
public sealed class AuthController(IOptions<AuthSettings> settings) : ControllerBase
{
    private SignInMode Mode => settings.Value.Mode;

    [HttpGet("session")]
    public SessionResponse GetSession()
    {
        if (User.Identity?.IsAuthenticated != true)
            return new SessionResponse(Mode, User: null);

        var sessionUser = new SessionUser(
            User.Identity.Name ?? string.Empty,
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
        [FromServices] UserSignIn userSignIn,
        CancellationToken cancellationToken)
    {
        if (Mode != SignInMode.Development)
            return NotFound();

        var identity = new ExternalIdentity($"dev:{request.Email.ToLowerInvariant()}", request.Email, request.DisplayName);
        var principal = await userSignIn.CreatePrincipalAsync(identity, request.IsOfficeManager, cancellationToken);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        return NoContent();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (Mode == SignInMode.Development)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect("/");
        }

        // Sign out of DeskShare (cookie) and of the identity provider, so SSO doesn't sign the user straight back in.
        var properties = new AuthenticationProperties { RedirectUri = "/" };
        return SignOut(properties, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
    }

    private string ToSafeLocalUrl(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
}
