using System.Reflection;
using Common.Application;
using TrackHub.Reporting.Application;
using TrackHub.Reporting.Application.Report.Factory;
using TrackHub.Reporting.Application.Report.Factory.Gps;
using TrackHub.Reporting.Application.Report.Factory.Admin;
using TrackHub.Reporting.Application.Report.Factory.Document;
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
        services.AddScoped<IReport, GeofenceEvents>();
        services.AddScoped<IReport, GpsProviderHealthSummaryReport>();
        services.AddScoped<IReport, GpsProviderSyncHistoryReport>();
        services.AddScoped<IReport, GpsSyncStatisticsReport>();
        services.AddScoped<IReport, GpsSynchronizedDeviceInventoryReport>();
        services.AddScoped<IReport, GpsRecentlyAddedDevicesReport>();
        services.AddScoped<IReport, GpsUnassignedDevicesReport>();
        services.AddScoped<IReport, GpsIgnoredDevicesReport>();
        services.AddScoped<IReport, GpsAssignmentHistoryReport>();
        services.AddScoped<IReport, GpsLatestPositionFreshnessReport>();
        services.AddScoped<IReport, GpsPositionHistoryReport>();
        // Account lifecycle / branding reports.
        services.AddScoped<IReport, AccountsByStatusReport>();
        services.AddScoped<IReport, FeatureEnablementMatrixReport>();
        services.AddScoped<IReport, GroupMembershipReport>();
        // Document reports.
        services.AddScoped<IReport, ExpiringDocumentsReport>();
        services.AddScoped<IReport, MissingRequiredDocumentsReport>();
        services.AddScoped<IReport, DocumentShareActivityReport>();
        services.AddScoped<IReport, DocumentUploadVolumeReport>();
        services.AddSingleton<IReportFactory, ReportFactory>();
        services.AddSingleton<IExcelHelper, ExcelHelper>();
        services.AddSingleton<IPdfReportBuilder, PdfReportBuilder>();

        return services;
    }
}

