using DeskShare.Application.Abstractions;
using DeskShare.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace DeskShare.Infrastructure.Persistence.Repositories;

internal sealed class EmployeeRepository(DeskShareDbContext dbContext) : IEmployeeRepository
{
    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Employees.FirstOrDefaultAsync(employee => employee.Id == id, cancellationToken);

    public Task<Employee?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken) =>
        dbContext.Employees.FirstOrDefaultAsync(employee => employee.ExternalId == externalId, cancellationToken);

    public void Add(Employee employee) => dbContext.Employees.Add(employee);
}
