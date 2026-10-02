using DeskShare.Application.Abstractions;
using DeskShare.Domain.Employees;

namespace DeskShare.Api.IntegrationTests;

// Simulates a broken dependency so sign-in fails with an unexpected 500.
internal sealed class FailingEmployeeRepository : IEmployeeRepository
{
    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("Simulated database failure.");

    public Task<Employee?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("Simulated database failure.");

    public void Add(Employee employee) =>
        throw new InvalidOperationException("Simulated database failure.");
}
