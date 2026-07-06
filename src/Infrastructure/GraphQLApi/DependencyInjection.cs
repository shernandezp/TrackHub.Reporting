using TrackHub.Reporting.Infrastructure.GraphQLApi;
using TrackHub.Reporting.Domain.Interfaces.Router;
using TrackHub.Reporting.Domain.Interfaces.Geofence;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Interfaces.Telemetry;
using TrackHub.Reporting.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddReportsContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHeaderPropagation(o => o.Headers.Add("Authorization"));

        services.AddHttpClient(Clients.Router,
            client => ConfigureGraphQLClient(client, configuration, Clients.Router))
            .AddHeaderPropagation()
            .AddStandardResilienceHandler();
        services.AddHttpClient(Clients.Geofence,
            client => ConfigureGraphQLClient(client, configuration, Clients.Geofence))
            .AddHeaderPropagation()
            .AddStandardResilienceHandler();
        services.AddHttpClient(Clients.Manager,
            client => ConfigureGraphQLClient(client, configuration, Clients.Manager))
            .AddHeaderPropagation()
            .AddStandardResilienceHandler();
        services.AddHttpClient(Clients.Telemetry,
            client => ConfigureGraphQLClient(client, configuration, Clients.Telemetry))
            .AddHeaderPropagation()
            .AddStandardResilienceHandler();

        services.AddScoped<IRouterReader, RouterReader>();
        services.AddScoped<IGeofenceReader, GeofenceReader>();
        services.AddScoped<IGpsManagerReader, GpsManagerReader>();
        services.AddScoped<IGpsTelemetryReader, GpsTelemetryReader>();
        services.AddScoped<IAccountFeatureReader, AccountFeatureReader>();
        services.AddScoped<IReportAuditWriter, ReportAuditWriter>();

        return services;
    }

    private static void ConfigureGraphQLClient(HttpClient client, IConfiguration configuration, string serviceName)
    {
        var url = configuration.GetValue<string>($"AppSettings:GraphQL{serviceName}Service")
            ?? throw new InvalidOperationException($"Setting 'GraphQL{serviceName}Service' not found.");
        client.BaseAddress = new Uri(url);
        client.Timeout = TimeSpan.FromSeconds(30);
    }
}

