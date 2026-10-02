using DeskShare.Application.Abstractions;
using DeskShare.Application.Common.Exceptions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DeskShare.Infrastructure.Persistence;

internal sealed class UnitOfWork(DeskShareDbContext dbContext) : IUnitOfWork
{
    private const int SqliteUniqueConstraintErrorCode = 2067;

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqliteException { SqliteExtendedErrorCode: SqliteUniqueConstraintErrorCode })
        {
            throw new ConflictException(
                "Your change conflicts with a change someone else just made. Please refresh and try again.",
                exception);
        }
    }
}
