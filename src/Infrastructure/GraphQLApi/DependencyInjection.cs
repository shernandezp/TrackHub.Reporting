using TrackHub.Reporting.Infrastructure.GraphQLApi;
using TrackHub.Reporting.Domain.Interfaces.Router;
using TrackHub.Reporting.Domain.Interfaces.Geofence;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddReportsContext(this IServiceCollection services)
    {
        services.AddHeaderPropagation(o => o.Headers.Add("Authorization"));

        services.AddHttpClient(Clients.Router,
            client => client.Timeout = TimeSpan.FromSeconds(30))
            .AddHeaderPropagation();
        services.AddHttpClient(Clients.Geofence,
            client => client.Timeout = TimeSpan.FromSeconds(30))
            .AddHeaderPropagation();

        services.AddScoped<IRouterReader, RouterReader>();
        services.AddScoped<IGeofenceReader, GeofenceReader>();

        return services;
    }
}
