using DeskShare.Application.Abstractions;
using DeskShare.Application.Common.Exceptions;
using DeskShare.Domain.OperationalLogs;
using Microsoft.AspNetCore.Diagnostics;

namespace DeskShare.Api.ErrorHandling;

// Records unexpected exceptions in the OperationalLogs table, then lets the next handler write the response.
public sealed class OperationalLogExceptionHandler(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<OperationalLogExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (IsExpected(exception) || IsCancelledByClient(exception, httpContext))
            return false;

        var log = OperationalLog.Create(exception, httpContext.Request.Path, timeProvider.GetUtcNow().UtcDateTime);

        try
        {
            // A fresh scope gives a clean DbContext, even if the request's own context is what failed.
            await using var scope = scopeFactory.CreateAsyncScope();
            var operationalLogs = scope.ServiceProvider.GetRequiredService<IOperationalLogRepository>();
            await operationalLogs.AddOrIncrementAsync(log, CancellationToken.None);
        }
        catch (Exception loggingFailure)
        {
            // Never let a logging problem replace the original error response.
            logger.LogError(loggingFailure, "Could not write operational log {Fingerprint}.", log.Fingerprint);
        }

        return false;
    }

    // Business answers such as "desk already booked" are normal behaviour, not operational problems.
    private static bool IsExpected(Exception exception) =>
        exception is BusinessRuleException or ConflictException or NotFoundException or ForbiddenException;

    private static bool IsCancelledByClient(Exception exception, HttpContext httpContext) =>
        exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested;
}
