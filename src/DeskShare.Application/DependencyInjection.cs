using DeskShare.Application.Bookings;
using DeskShare.Application.Desks;
using DeskShare.Application.Employees;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DeskShare.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddAutoMapper(config => config.AddProfile<MappingProfile>());

        services.AddScoped<DeskService>();
        services.AddScoped<BookingService>();
        services.AddScoped<EmployeeProvisioningService>();

        return services;
    }
}
