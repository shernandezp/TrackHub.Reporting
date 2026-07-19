using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Gps;

public sealed class GpsAssignmentHistoryReport(
    IUser user,
    IAccountFeatureReader features,
    IGpsManagerReader manager) : IReport
{
    public string ReportCode => Reports.GpsAssignmentHistory;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        var accountId = await GpsReportSupport.RequireAccountAsync(user, features, FeatureKeys.GpsIntegration, cancellationToken);
        var assignments = await manager.GetAssignmentsByAccountAsync(accountId, false, cancellationToken);
        IEnumerable<Domain.Models.Manager.ManagerTransporterDeviceAssignmentVm> filtered = assignments;
        if (Guid.TryParse(filters.StringFilter1, out var tx))
            filtered = filtered.Where(a => a.TransporterId == tx);
        if (filters.DateTimeFilter1.HasValue)
            filtered = filtered.Where(a => a.EffectiveFrom >= filters.DateTimeFilter1.Value);
        if (filters.DateTimeFilter2.HasValue)
            filtered = filtered.Where(a => a.EffectiveFrom <= filters.DateTimeFilter2.Value);
        var rows = filtered
            .OrderByDescending(a => a.EffectiveFrom)
            .Select(a => new GpsAssignmentHistoryRowVm(
                a.TransporterId,
                a.DeviceId,
                a.IsPrimary ? "Primary" : "Secondary",
                a.EffectiveFrom,
                a.EffectiveTo,
                a.AssignmentReason))
            .ToList();
        return ReportDataset.Create(filters, rows);
    }
}
