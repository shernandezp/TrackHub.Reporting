namespace TrackHub.Reporting.Domain.Exceptions;

public sealed class ReportLimitExceededException(int maxRows)
    : Exception($"Report exceeds the maximum allowed row count of {maxRows}. Please narrow your filters.")
{
    public string Code => "REPORT_ROW_LIMIT_EXCEEDED";
    public int MaxRows { get; } = maxRows;
}
