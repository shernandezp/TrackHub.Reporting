namespace TrackHub.Reporting.Domain.Models;

// The rendered report file returned by GetReportQuery: the bytes plus the metadata the endpoint
// needs to stream the download with the right content type and extension (spec 06 §7.2).
public readonly record struct ReportFileVm(byte[] Content, string ContentType, string FileExtension, int RowCount);
