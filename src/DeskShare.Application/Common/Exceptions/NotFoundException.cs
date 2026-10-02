namespace DeskShare.Application.Common.Exceptions;

public sealed class NotFoundException(string resourceName, Guid id)
    : Exception($"{resourceName} '{id}' was not found.");
