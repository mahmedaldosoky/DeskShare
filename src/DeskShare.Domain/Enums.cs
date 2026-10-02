namespace DeskShare.Domain;

[Flags]
public enum DeskFeatures
{
    None = 0,
    Monitor = 1,
    Standing = 2,
    Window = 4,
}

public enum SignInMode
{
    /// <summary>Local sign-in form with no identity provider. Only allowed in the Development environment.</summary>
    Development,

    /// <summary>Single sign-on through an OpenID Connect provider such as ADFS, Entra ID or Keycloak.</summary>
    Oidc,
}
