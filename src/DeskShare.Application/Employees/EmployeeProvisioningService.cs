using DeskShare.Application.Abstractions;
using DeskShare.Domain.Employees;

namespace DeskShare.Application.Employees;

public sealed class EmployeeProvisioningService(
    IEmployeeRepository employees,
    IUnitOfWork unitOfWork)
{
    public async Task<Guid> ProvisionAsync(ExternalIdentity identity, CancellationToken cancellationToken)
    {
        var employee = await employees.GetByExternalIdAsync(identity.ExternalId, cancellationToken);

        if (employee is null)
        {
            employee = Employee.Create(identity.ExternalId, identity.Email, identity.DisplayName);
            employees.Add(employee);
        }
        else
        {
            employee.UpdateProfile(identity.Email, identity.DisplayName);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return employee.Id;
    }
}
