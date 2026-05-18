namespace TrackHub.Reporting.Domain.Interfaces.Foundation;

public interface IPlatformFeatureReader
{
    Task EnsureFeatureEnabledAsync(Guid accountId, string featureKey, CancellationToken cancellationToken);
}

public interface IReportAuditWriter
{
    Task RecordReportExportAsync(
        Guid accountId,
        string actorType,
        string actorId,
        string reportCode,
        string filtersJson,
        int rowCount,
        string format,
        string? correlationId,
        CancellationToken cancellationToken);
}
