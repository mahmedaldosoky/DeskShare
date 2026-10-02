using DeskShare.Domain;

namespace DeskShare.Application.Desks;

public sealed record DeskDto(Guid Id, string Code, int Floor, IReadOnlyList<DeskFeatures> Features);
