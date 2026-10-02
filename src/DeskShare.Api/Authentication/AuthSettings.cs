namespace DeskShare.Api.Authentication;

public enum SignInMode
{
    /// <summary>Local sign-in form with no identity provider. Only allowed in the Development environment.</summary>
    Development,

    /// <summary>Single sign-on through an OpenID Connect provider such as Auth0, ADFS or Entra ID.</summary>
    Oidc,
}

/// <summary>The "Authentication" section of appsettings.json, which holds all the values.</summary>
public sealed class AuthSettings
{
    public const string SectionName = "Authentication";

    public SignInMode Mode { get; init; }

    public string Authority { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;

    /// <summary>Members of this group get the Office Manager role.</summary>
    public string OfficeManagerGroup { get; init; } = string.Empty;

    // Identity providers name these claims differently (ADFS: "upn", "unique_name", "group").
    public string EmailClaim { get; init; } = string.Empty;
    public string NameClaim { get; init; } = string.Empty;
    public string GroupsClaim { get; init; } = string.Empty;
}
