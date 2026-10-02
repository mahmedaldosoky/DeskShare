using DeskShare.Domain.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeskShare.Infrastructure.Persistence.Configurations;

internal sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(booking => booking.Id);

        builder.HasOne(booking => booking.Desk)
            .WithMany()
            .HasForeignKey(booking => booking.DeskId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(booking => booking.Employee)
            .WithMany()
            .HasForeignKey(booking => booking.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // The application checks these rules first for friendly errors;
        // the unique indexes guarantee them even under concurrent requests.
        builder.HasIndex(booking => new { booking.DeskId, booking.Date }).IsUnique();
        builder.HasIndex(booking => new { booking.EmployeeId, booking.Date }).IsUnique();
    }
}
