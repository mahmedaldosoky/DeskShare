using DeskShare.Domain;
using DeskShare.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeskShare.Infrastructure.Persistence.Configurations;

internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    private const int ExternalIdMaxLength = 200;

    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(employee => employee.Id);

        builder.Property(employee => employee.ExternalId)
            .HasMaxLength(ExternalIdMaxLength)
            .IsRequired();

        builder.Property(employee => employee.Email)
            .HasMaxLength(EmployeeLimits.EmailMaxLength)
            .IsRequired();

        builder.Property(employee => employee.DisplayName)
            .HasMaxLength(EmployeeLimits.DisplayNameMaxLength)
            .IsRequired();

        builder.HasIndex(employee => employee.ExternalId).IsUnique();
    }
}
