namespace TrackHub.Reporting.Domain.Models;

public readonly record struct ReportExportResult(byte[] Content, int RowCount);
