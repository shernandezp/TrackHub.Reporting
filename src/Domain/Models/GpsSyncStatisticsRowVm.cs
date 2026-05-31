namespace TrackHub.Reporting.Domain.Models;

public readonly record struct GpsSyncStatisticsRowVm(
    DateTime Date,
    string Operator,
    int Runs,
    int Successes,
    int Failures,
    double AvgDurationMs,
    int TotalPositions);
