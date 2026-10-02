using System.ComponentModel.DataAnnotations;
using DeskShare.Domain;

namespace DeskShare.Application.Desks;

public sealed record SaveDeskRequest(
    [Required(AllowEmptyStrings = false), StringLength(DeskLimits.CodeMaxLength)] string Code,
    [Range(DeskLimits.MinFloor, DeskLimits.MaxFloor)] int Floor,
    IReadOnlyList<DeskFeatures>? Features)
{
    public DeskFeatures CombineFeatures() =>
        (Features ?? []).Aggregate(DeskFeatures.None, (combined, feature) => combined | feature);
}
