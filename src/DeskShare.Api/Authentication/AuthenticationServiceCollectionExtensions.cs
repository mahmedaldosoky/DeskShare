using DeskShare.Application.Abstractions;
using DeskShare.Domain;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace DeskShare.Api.Authentication;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddDeskShareAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var section = configuration.GetSection(DeskShareAuthenticationOptions.SectionName);
        var options = section.Get<DeskShareAuthenticationOptions>() ?? new DeskShareAuthenticationOptions();
        EnsureOptionsAreValid(options, environment);

        services.Configure<DeskShareAuthenticationOptions>(section);
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<DeskSharePrincipalFactory>();

        var authentication = services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(ConfigureCookie);

        if (options.Mode == SignInMode.Oidc)
        {
            services.AddScoped<DeskShareOidcEvents>();
            authentication.AddOpenIdConnect(oidc => ConfigureOpenIdConnect(oidc, options.Oidc));
        }

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

    private static void ConfigureOpenIdConnect(OpenIdConnectOptions oidc, OidcSettings settings)
    {
        oidc.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        oidc.Authority = settings.Authority;
        oidc.ClientId = settings.ClientId;
        oidc.ClientSecret = settings.ClientSecret;
        oidc.ResponseType = "code";
        oidc.UsePkce = true;
        oidc.MapInboundClaims = false;
        oidc.SaveTokens = false;
        oidc.EventsType = typeof(DeskShareOidcEvents);

        oidc.Scope.Clear();
        foreach (var scope in settings.Scopes)
            oidc.Scope.Add(scope);
    }

    private static Task WriteStatusCode(HttpResponse response, int statusCode)
    {
        response.StatusCode = statusCode;
        return Task.CompletedTask;
    }

    private static void EnsureOptionsAreValid(DeskShareAuthenticationOptions options, IHostEnvironment environment)
    {
        if (options.Mode == SignInMode.Development && !environment.IsDevelopment())
            throw new InvalidOperationException("Development sign-in can only be used in the Development environment.");

        if (options.Mode == SignInMode.Oidc &&
            (string.IsNullOrWhiteSpace(options.Oidc.Authority) || string.IsNullOrWhiteSpace(options.Oidc.ClientId)))
            throw new InvalidOperationException("Authentication:Oidc:Authority and Authentication:Oidc:ClientId are required for OIDC sign-in.");
    }
}
