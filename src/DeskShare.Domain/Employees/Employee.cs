using DeskShare.Domain.Common;

namespace DeskShare.Domain.Employees;

public sealed class Employee : AuditedEntity
{
    public string ExternalId { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;

    private Employee()
    {
    }

    public static Employee Create(string externalId, string email, string displayName)
    {
        var employee = new Employee { ExternalId = externalId.Trim() };
        employee.UpdateProfile(email, displayName);
        return employee;
    }

    public void UpdateProfile(string email, string displayName)
    {
        Email = email.Trim();
        DisplayName = displayName.Trim();
    }
}
