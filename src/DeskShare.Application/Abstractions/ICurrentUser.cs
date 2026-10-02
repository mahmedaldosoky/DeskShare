namespace DeskShare.Application.Abstractions;

public interface ICurrentUser
{
    /// <summary>The signed-in employee. Throws when nobody is signed in.</summary>
    Guid EmployeeId { get; }

    /// <summary>The signed-in employee, or null during sign-in, startup or background work.</summary>
    Guid? FindEmployeeId();
}
