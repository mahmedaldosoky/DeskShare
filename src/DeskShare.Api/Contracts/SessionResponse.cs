using DeskShare.Api.Authentication;

namespace DeskShare.Api.Contracts;

public sealed record SessionResponse(SignInMode SignInMode, SessionUser? User);

public sealed record SessionUser(string DisplayName, IReadOnlyList<string> Roles);
