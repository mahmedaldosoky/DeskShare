using DeskShare.Domain.OperationalLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeskShare.Infrastructure.Persistence.Configurations;

internal sealed class OperationalLogConfiguration : IEntityTypeConfiguration<OperationalLog>
{
    private const int FingerprintLength = 64;
    private const int ExceptionTypeMaxLength = 500;
    private const int SourceMaxLength = 1000;
    private const int RequestPathMaxLength = 2048;

    public void Configure(EntityTypeBuilder<OperationalLog> builder)
    {
        builder.HasKey(log => log.Fingerprint);

        builder.Property(log => log.Fingerprint).HasMaxLength(FingerprintLength);
        builder.Property(log => log.ExceptionType).HasMaxLength(ExceptionTypeMaxLength).IsRequired();
        builder.Property(log => log.Source).HasMaxLength(SourceMaxLength).IsRequired();
        builder.Property(log => log.Message).IsRequired();
        builder.Property(log => log.RequestPath).HasMaxLength(RequestPathMaxLength).IsRequired();
    }
}
