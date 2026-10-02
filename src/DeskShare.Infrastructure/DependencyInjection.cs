using DeskShare.Application.Abstractions;
using DeskShare.Infrastructure.Persistence;
using DeskShare.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DeskShare.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<AuditingInterceptor>();
        services.AddDbContext<DeskShareDbContext>((serviceProvider, options) => options
            .UseSqlite(connectionString)
            .AddInterceptors(serviceProvider.GetRequiredService<AuditingInterceptor>()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDeskRepository, DeskRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IOperationalLogRepository, OperationalLogRepository>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider services, bool seedSampleData)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DeskShareDbContext>();

        await dbContext.Database.MigrateAsync();

        if (seedSampleData)
            await SampleDataSeeder.SeedAsync(dbContext, CancellationToken.None);
    }
}
