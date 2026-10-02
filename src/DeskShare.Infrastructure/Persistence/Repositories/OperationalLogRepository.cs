using DeskShare.Application.Abstractions;
using DeskShare.Domain.OperationalLogs;
using Microsoft.EntityFrameworkCore;

namespace DeskShare.Infrastructure.Persistence.Repositories;

internal sealed class OperationalLogRepository(DeskShareDbContext dbContext) : IOperationalLogRepository
{
    // A single upsert statement is atomic, so two requests failing at the same moment cannot create duplicates.
    public Task AddOrIncrementAsync(OperationalLog log, CancellationToken cancellationToken) =>
        dbContext.Database.ExecuteSqlAsync(
            $"""
            INSERT INTO "OperationalLogs"
                ("Fingerprint", "ExceptionType", "Source", "Message", "StackTrace", "RequestPath",
                 "OccurrenceCount", "FirstOccurredAtUtc", "LastOccurredAtUtc")
            VALUES
                ({log.Fingerprint}, {log.ExceptionType}, {log.Source}, {log.Message}, {log.StackTrace}, {log.RequestPath},
                 {log.OccurrenceCount}, {log.FirstOccurredAtUtc}, {log.LastOccurredAtUtc})
            ON CONFLICT ("Fingerprint") DO UPDATE SET
                "OccurrenceCount" = "OccurrenceCount" + 1,
                "Message" = excluded."Message",
                "StackTrace" = excluded."StackTrace",
                "RequestPath" = excluded."RequestPath",
                "LastOccurredAtUtc" = excluded."LastOccurredAtUtc"
            """,
            cancellationToken);
}
