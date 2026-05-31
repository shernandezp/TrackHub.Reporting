using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Helpers;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Gps;

public sealed class GpsLatestPositionFreshnessReport(
    IUser user,
    IAccountFeatureReader features,
    IGpsManagerReader manager,
    IExcelHelper helper) : IReport
{
    public string ReportCode => Reports.GpsLatestPositionFreshness;

    public async Task<ReportExportResult> GenerateAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        _ = await GpsReportSupport.RequireAccountAsync(user, features, FeatureKeys.GpsIntegration, cancellationToken);
        var operators = await manager.GetOperatorsAsync(cancellationToken);
        var rows = new List<GpsLatestPositionFreshnessRowVm>();
        var now = DateTimeOffset.UtcNow;
        foreach (var op in operators.Where(o => o.Enabled))
        {
            IReadOnlyCollection<Domain.Models.Manager.ManagerTransporterPositionVm> positions;
            try
            {
                positions = await manager.GetLatestPositionsAsync(op.OperatorId, cancellationToken);
            }
            catch
            {
                continue;
            }
            foreach (var p in positions)
            {
                rows.Add(new GpsLatestPositionFreshnessRowVm(
                    p.TransporterId,
                    p.DeviceName,
                    p.DeviceDateTime,
                    Math.Round((now - p.DeviceDateTime).TotalMinutes, 2),
                    op.Name));
            }
        }
        var bytes = GpsReportSupport.Export(helper, filters, rows);
        return new ReportExportResult(bytes, rows.Count);
    }
}
