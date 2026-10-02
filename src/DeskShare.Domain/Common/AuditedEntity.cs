namespace DeskShare.Domain.Common;

// Filled in automatically when changes are saved.
public abstract class AuditedEntity : Entity
{
    public DateTime CreationTime { get; private set; }
    public Guid? CreatorId { get; private set; }
    public DateTime? LastModificationTime { get; private set; }
    public Guid? LastModifierId { get; private set; }
}
