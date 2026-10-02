using System.Text.Json.Serialization;
using DeskShare.Api.ErrorHandling;

namespace DeskShare.Api;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(json => json.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));

        services.AddProblemDetails();
        services.AddExceptionHandler<OperationalLogExceptionHandler>();
        services.AddExceptionHandler<ApplicationExceptionHandler>();
        services.AddOpenApi();

        return services;
    }
}
