using DeskShare.Domain.OperationalLogs;

namespace DeskShare.UnitTests.Domain;

public sealed class OperationalLogTests
{
    private static readonly DateTime Now = new(2026, 10, 2, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void SameExceptionFromSameMethod_HasSameFingerprint()
    {
        var first = OperationalLog.Create(Capture(() => ThrowInvalidOperation("first")), "/a", Now);
        var second = OperationalLog.Create(Capture(() => ThrowInvalidOperation("second")), "/b", Now);

        Assert.Equal(first.Fingerprint, second.Fingerprint);
    }

    [Fact]
    public void DifferentExceptionType_HasDifferentFingerprint()
    {
        var invalidOperation = OperationalLog.Create(Capture(() => ThrowInvalidOperation("x")), "/a", Now);
        var argument = OperationalLog.Create(Capture(() => ThrowArgument("x")), "/a", Now);

        Assert.NotEqual(invalidOperation.Fingerprint, argument.Fingerprint);
    }

    [Fact]
    public void SameExceptionFromDifferentMethod_HasDifferentFingerprint()
    {
        var fromOneMethod = OperationalLog.Create(Capture(() => ThrowInvalidOperation("x")), "/a", Now);
        var fromAnotherMethod = OperationalLog.Create(Capture(() => ThrowInvalidOperationElsewhere("x")), "/a", Now);

        Assert.NotEqual(fromOneMethod.Fingerprint, fromAnotherMethod.Fingerprint);
    }

    [Fact]
    public void Create_StartsWithOneOccurrence()
    {
        var log = OperationalLog.Create(Capture(() => ThrowInvalidOperation("boom")), "/api/desks", Now);

        Assert.Equal(1, log.OccurrenceCount);
        Assert.Equal("boom", log.Message);
        Assert.Equal("/api/desks", log.RequestPath);
        Assert.Equal(typeof(InvalidOperationException).FullName, log.ExceptionType);
        Assert.EndsWith($"{nameof(OperationalLogTests)}.{nameof(ThrowInvalidOperation)}", log.Source);
    }

    private static Exception Capture(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            return exception;
        }

        throw new InvalidOperationException("Expected the action to throw.");
    }

    private static void ThrowInvalidOperation(string message) => throw new InvalidOperationException(message);

    private static void ThrowInvalidOperationElsewhere(string message) => throw new InvalidOperationException(message);

    private static void ThrowArgument(string message) => throw new ArgumentException(message);
}
