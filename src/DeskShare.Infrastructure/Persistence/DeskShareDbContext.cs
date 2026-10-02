using DeskShare.Domain.Bookings;
using DeskShare.Domain.Desks;
using DeskShare.Domain.Employees;
using DeskShare.Domain.OperationalLogs;
using Microsoft.EntityFrameworkCore;

namespace DeskShare.Infrastructure.Persistence;

public sealed class DeskShareDbContext(DbContextOptions<DeskShareDbContext> options) : DbContext(options)
{
    public DbSet<Desk> Desks => Set<Desk>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<OperationalLog> OperationalLogs => Set<OperationalLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeskShareDbContext).Assembly);
}
