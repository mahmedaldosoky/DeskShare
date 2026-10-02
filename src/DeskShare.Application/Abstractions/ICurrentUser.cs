namespace DeskShare.Application.Abstractions;

public interface ICurrentUser
{
    // Throws when nobody is signed in.
    Guid EmployeeId { get; }

    // Null during sign-in, startup or background work.
    Guid? FindEmployeeId();
}
