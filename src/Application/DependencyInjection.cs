using System.Reflection;
using Common.Application;
using TrackHub.Reporting.Application;
using TrackHub.Reporting.Application.Report.Factory;
using TrackHub.Reporting.Domain.Helpers;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Helpers;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddApplicationServices(assembly, false);
        services.AddDistributedMemoryCache();

        services.AddScoped<IReport, LiveReport>();
        services.AddScoped<IReport, PositionRecord>();
        services.AddScoped<IReport, TransportersInGeofence>();
        services.AddSingleton<IReportFactory, ReportFactory>();
        services.AddSingleton<IExcelHelper, ExcelHelper>();

        return services;
    }
}
