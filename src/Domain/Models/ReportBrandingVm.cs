namespace TrackHub.Reporting.Domain.Models;

// Account branding (spec 03) fetched from the Manager catalog for the PDF header block (spec 06 §6/§7.2).
// Both members are optional: an account without branding, or a branding fetch that fails, yields nulls and
// the PDF simply renders without the branding block (a branding failure must never fail the export).
public readonly record struct ReportBrandingVm(string? AccountName, byte[]? LogoImage);
