// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//

using System.Globalization;
using Common.Application.Interfaces;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Helpers;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Gps;

internal static class GpsReportSupport
{
    public const int DefaultPageSize = 5000;
    public const int MaxExportRows = 100_000;

    public static async Task<Guid> RequireAccountAsync(IUser user, IAccountFeatureReader features, string featureKey, CancellationToken ct)
    {
        var accountId = user.AccountId ?? throw new UnauthorizedAccessException();
        await features.EnsureFeatureEnabledAsync(accountId, featureKey, ct);
        return accountId;
    }

    public static byte[] Export<T>(IExcelHelper helper, FilterDto filters, ICollection<T> rows)
    {
        var culture = new CultureInfo(filters.Language);
        return helper.Export(filters.Name, filters.DateTimeFilter1, filters.DateTimeFilter2, rows, culture);
    }

    public static int ResolveTake(FilterDto filters, int defaultTake = DefaultPageSize)
    {
        if (!filters.NumericFilter1.HasValue || filters.NumericFilter1.Value <= 0)
        {
            return defaultTake;
        }

        return (int)Math.Min(filters.NumericFilter1.Value, MaxExportRows);
    }
}
