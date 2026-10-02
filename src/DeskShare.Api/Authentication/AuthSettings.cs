namespace DeskShare.Api.Authentication;

public enum SignInMode
{
    // Local sign-in form with no identity provider. Only allowed in the Development environment.
    Development,

    Oidc,
}

public sealed class AuthSettings
{
    public const string SectionName = "Authentication";

    public SignInMode Mode { get; init; }

    public string Authority { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;

    public string OfficeManagerGroup { get; init; } = string.Empty;

    // Identity providers name these claims differently (ADFS: "upn", "unique_name", "group").
    public string EmailClaim { get; init; } = string.Empty;
    public string NameClaim { get; init; } = string.Empty;
    public string GroupsClaim { get; init; } = string.Empty;
}
