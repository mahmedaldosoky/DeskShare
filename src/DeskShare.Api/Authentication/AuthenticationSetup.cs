using System.Security.Claims;
using DeskShare.Application.Abstractions;
using DeskShare.Application.Employees;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace DeskShare.Api.Authentication;

public static class AuthenticationSetup
{
    public static IServiceCollection AddDeskShareAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var section = configuration.GetSection(AuthSettings.SectionName);
        var settings = section.Get<AuthSettings>()
            ?? throw new InvalidOperationException($"The '{AuthSettings.SectionName}' section is missing from appsettings.json.");
        EnsureSettingsAreValid(settings, environment);

        services.Configure<AuthSettings>(section);
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<UserSignIn>();

        var authentication = services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(ConfigureCookie);

        if (settings.Mode == SignInMode.Oidc)
            authentication.AddOpenIdConnect(oidc => ConfigureOpenIdConnect(oidc, settings));

        services.AddAuthorization();

        return services;
    }

    private static void ConfigureCookie(CookieAuthenticationOptions cookie)
    {
        cookie.Cookie.Name = "DeskShare.Auth";
        cookie.Cookie.HttpOnly = true;
        cookie.Cookie.SameSite = SameSiteMode.Lax;
        cookie.ExpireTimeSpan = TimeSpan.FromHours(8);
        cookie.SlidingExpiration = true;

        // This is an API: answer with status codes instead of redirecting to a login page.
        cookie.Events.OnRedirectToLogin = context => WriteStatusCode(context.Response, StatusCodes.Status401Unauthorized);
        cookie.Events.OnRedirectToAccessDenied = context => WriteStatusCode(context.Response, StatusCodes.Status403Forbidden);
    }

    private static void ConfigureOpenIdConnect(OpenIdConnectOptions oidc, AuthSettings settings)
    {
        oidc.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        oidc.Authority = settings.Authority;
        oidc.ClientId = settings.ClientId;
        oidc.ClientSecret = settings.ClientSecret;
        oidc.ResponseType = "code";
        oidc.UsePkce = true;
        oidc.MapInboundClaims = false;
        oidc.SaveTokens = false;

        oidc.Scope.Clear();
        oidc.Scope.Add("openid");
        oidc.Scope.Add("profile");
        oidc.Scope.Add("email");

        // The provider's token is validated by now; swap it for our own small DeskShare user.
        oidc.Events.OnTokenValidated = async context =>
        {
            var token = context.Principal!;
            var email = token.FindFirstValue(settings.EmailClaim)
                ?? throw new InvalidOperationException($"The identity provider did not send a '{settings.EmailClaim}' claim.");

            var identity = new ExternalIdentity(
                ExternalId: token.FindFirstValue("sub")!,
                Email: email,
                DisplayName: token.FindFirstValue(settings.NameClaim) ?? email);

            var isOfficeManager = token.FindAll(settings.GroupsClaim).Any(group =>
                string.Equals(group.Value, settings.OfficeManagerGroup, StringComparison.OrdinalIgnoreCase));

            var userSignIn = context.HttpContext.RequestServices.GetRequiredService<UserSignIn>();
            context.Principal = await userSignIn.CreatePrincipalAsync(identity, isOfficeManager, context.HttpContext.RequestAborted);
        };

        // No id_token is saved, so client_id tells the provider which app's logout URLs to allow.
        oidc.Events.OnRedirectToIdentityProviderForSignOut = context =>
        {
            context.ProtocolMessage.ClientId = settings.ClientId;
            return Task.CompletedTask;
        };
    }

    private static Task WriteStatusCode(HttpResponse response, int statusCode)
    {
        response.StatusCode = statusCode;
        return Task.CompletedTask;
    }

    private static void EnsureSettingsAreValid(AuthSettings settings, IHostEnvironment environment)
    {
        if (settings.Mode == SignInMode.Development && !environment.IsDevelopment())
            throw new InvalidOperationException("Development sign-in can only be used in the Development environment.");

        if (settings.Mode != SignInMode.Oidc)
            return;

        var requiredForSso = new Dictionary<string, string>
        {
            [nameof(settings.Authority)] = settings.Authority,
            [nameof(settings.ClientId)] = settings.ClientId,
            [nameof(settings.OfficeManagerGroup)] = settings.OfficeManagerGroup,
            [nameof(settings.EmailClaim)] = settings.EmailClaim,
            [nameof(settings.NameClaim)] = settings.NameClaim,
            [nameof(settings.GroupsClaim)] = settings.GroupsClaim,
        };

        var missingKeys = requiredForSso
            .Where(setting => string.IsNullOrWhiteSpace(setting.Value))
            .Select(setting => $"{AuthSettings.SectionName}:{setting.Key}")
            .ToList();

        if (missingKeys.Count > 0)
            throw new InvalidOperationException($"SSO needs these settings: {string.Join(", ", missingKeys)}.");
    }
}
