using DeskShare.Domain.Common;

namespace DeskShare.Domain.Desks;

public sealed class Desk : AuditedEntity
{
    public string Code { get; private set; } = string.Empty;
    public int Floor { get; private set; }
    public DeskFeatures Features { get; private set; }

    // Deleted desks are kept so that past bookings still show which desk was used.
    public bool IsDeleted { get; private set; }

    private Desk()
    {
    }

    public static Desk Create(string code, int floor, DeskFeatures features)
    {
        var desk = new Desk();
        desk.UpdateDetails(code, floor, features);
        return desk;
    }

    public void UpdateDetails(string code, int floor, DeskFeatures features)
    {
        Code = code.Trim().ToUpperInvariant();
        Floor = floor;
        Features = features;
    }

    public void Delete() => IsDeleted = true;
}
