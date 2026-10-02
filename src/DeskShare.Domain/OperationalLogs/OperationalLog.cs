using System.Security.Cryptography;
using System.Text;

namespace DeskShare.Domain.OperationalLogs;

// One row per distinct error: the same exception type thrown from the same method shares a fingerprint,
// so repeats only increase OccurrenceCount instead of adding rows.
public sealed class OperationalLog
{
    public string Fingerprint { get; private set; } = string.Empty;
    public string ExceptionType { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string? StackTrace { get; private set; }
    public string RequestPath { get; private set; } = string.Empty;
    public int OccurrenceCount { get; private set; }
    public DateTime FirstOccurredAtUtc { get; private set; }
    public DateTime LastOccurredAtUtc { get; private set; }

    private OperationalLog()
    {
    }

    public static OperationalLog Create(Exception exception, string requestPath, DateTime occurredAtUtc)
    {
        var exceptionType = exception.GetType().FullName ?? exception.GetType().Name;
        var source = $"{exception.TargetSite?.DeclaringType?.FullName}.{exception.TargetSite?.Name}";

        return new OperationalLog
        {
            Fingerprint = CreateFingerprint(exceptionType, source),
            ExceptionType = exceptionType,
            Source = source,
            Message = exception.Message,
            StackTrace = exception.StackTrace,
            RequestPath = requestPath,
            OccurrenceCount = 1,
            FirstOccurredAtUtc = occurredAtUtc,
            LastOccurredAtUtc = occurredAtUtc,
        };
    }

    private static string CreateFingerprint(string exceptionType, string source) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{exceptionType}|{source}")));
}
