using TrackHub.Reporting.Domain.Records;
using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Domain.Interfaces.Factory;

public interface IReport
{
    string ReportCode { get; }
    Task<ReportExportResult> GenerateAsync(FilterDto filters, CancellationToken cancellationToken);
}
