using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Helpers;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Gps;

public sealed class GpsProviderHealthSummaryReport(
    IUser user,
    IAccountFeatureReader features,
    IGpsManagerReader manager,
    IExcelHelper helper) : IReport
{
    public string ReportCode => Reports.GpsProviderHealthSummary;

    public async Task<ReportExportResult> GenerateAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        _ = await GpsReportSupport.RequireAccountAsync(user, features, FeatureKeys.GpsOperatorHealth, cancellationToken);

        var lookbackHours = filters.NumericFilter1.HasValue && filters.NumericFilter1.Value > 0
            ? (int)Math.Min(filters.NumericFilter1.Value, 24 * 90)
            : 24;

        var operators = await manager.GetOperatorsAsync(cancellationToken);
        var rows = new List<GpsProviderHealthRowVm>(operators.Count);
        foreach (var op in operators)
        {
            var summary = await manager.GetOperatorHealthSummaryAsync(op.OperatorId, lookbackHours, cancellationToken);
            var status = summary.OfflineChecks > 0 && summary.HealthyChecks == 0
                ? "Offline"
                : summary.FailureCount > 0 ? "Degraded" : "Healthy";
            rows.Add(new GpsProviderHealthRowVm(
                op.OperatorId,
                op.Name,
                status,
                summary.UptimePercent,
                summary.AverageLatencyMs,
                summary.FailureCount,
                summary.LastCheckAt,
                summary.LastFailureCode));
        }
        var bytes = GpsReportSupport.Export(helper, filters, rows);
        return new ReportExportResult(bytes, rows.Count);
    }
}
