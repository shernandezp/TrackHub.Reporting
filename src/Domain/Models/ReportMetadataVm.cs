namespace TrackHub.Reporting.Domain.Models;

// Governed-catalog metadata for a report, read from the Manager catalog (reportByCode) and used by the
// execution pipeline to enforce feature/role visibility at run time (spec 06 §6, §7.2, §17.2).
public readonly record struct ReportMetadataVm(
    string Code,
    string? Description,
    string Category,
    string? RequiredFeatureKey,
    bool ManagerOnly,
    bool SupportsPdf,
    int SortOrder,
    bool Active);
