using DeskShare.Application.Abstractions;
using DeskShare.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DeskShare.Infrastructure.Persistence;

internal sealed class AuditingInterceptor(ICurrentUser currentUser, TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditFields(DbContext? dbContext)
    {
        if (dbContext is null)
            return;

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var userId = currentUser.FindEmployeeId();

        foreach (var entry in dbContext.ChangeTracker.Entries<AuditedEntity>().ToList())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    SetValue(entry, nameof(AuditedEntity.CreationTime), now);
                    SetValue(entry, nameof(AuditedEntity.CreatorId), userId);
                    break;

                case EntityState.Modified:
                    SetValue(entry, nameof(AuditedEntity.LastModificationTime), now);
                    SetValue(entry, nameof(AuditedEntity.LastModifierId), userId);
                    break;
            }
        }
    }

    private static void SetValue(EntityEntry entry, string propertyName, object? value) =>
        entry.Property(propertyName).CurrentValue = value;
}
