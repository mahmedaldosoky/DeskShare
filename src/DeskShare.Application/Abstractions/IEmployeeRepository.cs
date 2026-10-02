using DeskShare.Domain.Employees;

namespace DeskShare.Application.Abstractions;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Employee?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken);
    void Add(Employee employee);
}
