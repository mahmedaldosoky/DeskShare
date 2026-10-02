using DeskShare.Application.Employees;
using DeskShare.UnitTests.Fakes;

namespace DeskShare.UnitTests.Application;

public sealed class EmployeeProvisioningServiceTests
{
    private readonly InMemoryStore _store = new();
    private readonly EmployeeProvisioningService _service;

    public EmployeeProvisioningServiceTests()
    {
        _service = new EmployeeProvisioningService(new InMemoryEmployeeRepository(_store), new CountingUnitOfWork());
    }

    [Fact]
    public async Task FirstSignIn_CreatesProfileFromNameAndEmail()
    {
        var employeeId = await _service.ProvisionAsync(
            new ExternalIdentity("sub-1", "sara@contoso.com", "Sara Ali"), CancellationToken.None);

        var employee = Assert.Single(_store.Employees);
        Assert.Equal(employeeId, employee.Id);
        Assert.Equal("Sara Ali", employee.DisplayName);
        Assert.Equal("sara@contoso.com", employee.Email);
    }

    [Fact]
    public async Task LaterSignIn_UpdatesExistingProfile()
    {
        var firstId = await _service.ProvisionAsync(
            new ExternalIdentity("sub-1", "sara@contoso.com", "Sara Ali"), CancellationToken.None);

        var secondId = await _service.ProvisionAsync(
            new ExternalIdentity("sub-1", "sara.ali@contoso.com", "Sara A. Ali"), CancellationToken.None);

        Assert.Equal(firstId, secondId);
        var employee = Assert.Single(_store.Employees);
        Assert.Equal("Sara A. Ali", employee.DisplayName);
        Assert.Equal("sara.ali@contoso.com", employee.Email);
    }
}
