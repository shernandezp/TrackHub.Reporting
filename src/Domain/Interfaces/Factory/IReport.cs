using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Domain.Interfaces.Factory;

public interface IReport
{
    string ReportCode { get; }
    Task<byte[]> GenerateAsync(FilterDto filters, CancellationToken cancellationToken);
}
