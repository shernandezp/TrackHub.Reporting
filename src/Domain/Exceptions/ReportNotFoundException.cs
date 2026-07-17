namespace TrackHub.Reporting.Domain.Exceptions;

// The requested report code is unknown or inactive in the governed catalog (spec 06 §7.4 → 404).
public sealed class ReportNotFoundException(string reportCode)
    : Exception($"Report '{reportCode}' was not found.")
{
    public string Code => "REPORT_NOT_FOUND";
    public string ReportCode { get; } = reportCode;
}
