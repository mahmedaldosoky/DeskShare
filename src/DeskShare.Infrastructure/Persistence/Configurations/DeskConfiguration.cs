using DeskShare.Domain;
using DeskShare.Domain.Desks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeskShare.Infrastructure.Persistence.Configurations;

internal sealed class DeskConfiguration : IEntityTypeConfiguration<Desk>
{
    public void Configure(EntityTypeBuilder<Desk> builder)
    {
        builder.HasKey(desk => desk.Id);

        builder.Property(desk => desk.Code)
            .HasMaxLength(DeskLimits.CodeMaxLength)
            .IsRequired();

        // A deleted desk frees its code for reuse.
        builder.HasIndex(desk => desk.Code).IsUnique().HasFilter("\"IsDeleted\" = 0");
    }
}
