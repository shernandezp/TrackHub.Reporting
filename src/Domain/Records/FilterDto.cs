namespace TrackHub.Reporting.Domain.Records;

public record struct FilterDto(
    string Name,
    string Language,
    string StringFilter1,
    string StringFilter2,
    string StringFilter3,
    DateTimeOffset? DateTimeFilter1,
    DateTimeOffset? DateTimeFilter2,
    DateTimeOffset? DateTimeFilter3,
    double? NumericFilter1,
    double? NumericFilter2,
    double? NumericFilter3
);
