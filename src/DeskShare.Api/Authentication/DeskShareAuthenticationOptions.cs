using DeskShare.Domain;

namespace DeskShare.Api.Authentication;

public sealed class DeskShareAuthenticationOptions
{
    public const string SectionName = "Authentication";

    public SignInMode Mode { get; init; } = SignInMode.Oidc;

    /// <summary>Company groups whose members get the Office Manager role.</summary>
    public string[] OfficeManagerGroups { get; init; } = [];

    public OidcSettings Oidc { get; init; } = new();
}

public sealed class OidcSettings
{
    public string Authority { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string[] Scopes { get; init; } = ["openid", "profile", "email"];
    public OidcClaimTypes ClaimTypes { get; init; } = new();
}

/// <summary>Identity providers name their claims differently (e.g. ADFS uses "upn" and "group").</summary>
public sealed class OidcClaimTypes
{
    public string Subject { get; init; } = "sub";
    public string Email { get; init; } = "email";
    public string Name { get; init; } = "name";
    public string Groups { get; init; } = "groups";
}
