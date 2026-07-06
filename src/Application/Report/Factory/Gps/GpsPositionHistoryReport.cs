using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Helpers;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Interfaces.Telemetry;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Gps;

public sealed class GpsPositionHistoryReport(
    IUser user,
    IAccountFeatureReader features,
    IGpsTelemetryReader telemetry,
    IExcelHelper helper) : IReport
{
    public string ReportCode => Reports.GpsPositionHistory;

    public async Task<ReportExportResult> GenerateAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        var accountId = await GpsReportSupport.RequireAccountAsync(user, features, FeatureKeys.GpsPositionHistory, cancellationToken);
        Guid? transporterId = Guid.TryParse(filters.StringFilter1, out var t) ? t : null;
        Guid? deviceId = Guid.TryParse(filters.StringFilter2, out var d) ? d : null;
        var take = GpsReportSupport.ResolveTake(filters);
        var history = await telemetry.GetPositionHistoryAsync(accountId, transporterId, deviceId, take, cancellationToken);
        IEnumerable<Domain.Models.Manager.ManagerTransporterPositionHistoryVm> filtered = history;
        if (filters.DateTimeFilter1.HasValue)
            filtered = filtered.Where(p => p.SourceTimestamp >= filters.DateTimeFilter1.Value);
        if (filters.DateTimeFilter2.HasValue)
            filtered = filtered.Where(p => p.SourceTimestamp <= filters.DateTimeFilter2.Value);
        var rows = filtered
            .OrderByDescending(p => p.SourceTimestamp)
            .Select(p => new GpsPositionHistoryRowVm(
                p.TransporterId,
                p.SourceTimestamp,
                p.Latitude,
                p.Longitude,
                p.DeviceId,
                p.AccountId))
            .ToList();
        var bytes = GpsReportSupport.Export(helper, filters, rows);
        return new ReportExportResult(bytes, rows.Count);
    }
}
