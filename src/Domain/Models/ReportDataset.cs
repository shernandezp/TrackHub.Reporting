// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using System.Globalization;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Domain.Models;

// One column of a report dataset. PropertyName is the resx key used to resolve the localized
// header at format time (ExcelHelper / PdfReportBuilder), matching the pre-refactor behavior.
public readonly record struct ReportColumn(string PropertyName, Type PropertyType);

// The format-agnostic tabular result of running a report. Produced once per report
// (IReport.GetDatasetAsync) and consumed by every output format: Excel, PDF, and JSON preview.
public sealed class ReportDataset
{
    public required string Title { get; init; }
    public DateTimeOffset? FromDate { get; init; }
    public DateTimeOffset? ToDate { get; init; }
    public DateTimeOffset GeneratedAt { get; init; }
    public required IReadOnlyList<ReportColumn> Columns { get; init; }
    public required IReadOnlyList<object?[]> Rows { get; init; }
    public IReadOnlyList<KeyValuePair<string, string>> AppliedFilters { get; init; } = [];

    // Optional account branding rendered on the PDF header block when present.
    public string? AccountName { get; init; }
    public byte[]? LogoImage { get; init; }

    public int RowCount => Rows.Count;

    // Returns a copy of this dataset with the account branding block populated. Used by
    // the export pipeline to attach branding fetched from Manager to a PDF export without the report itself
    // needing to know about branding. All other fields (data, columns, filters) are carried over unchanged.
    public ReportDataset WithBranding(string? accountName, byte[]? logoImage) => new()
    {
        Title = Title,
        FromDate = FromDate,
        ToDate = ToDate,
        GeneratedAt = GeneratedAt,
        Columns = Columns,
        Rows = Rows,
        AppliedFilters = AppliedFilters,
        AccountName = accountName,
        LogoImage = logoImage
    };

    // Generic factory: reflects T's public instance properties (declaration order) into Columns and
    // flattens each row into an object?[] in the same order. Title/date range/applied-filters are
    // derived from the FilterDto. This is the single reflection step both Excel and PDF share, so the
    // column order stays identical to the pre-refactor ClosedXML InsertTable output.
    public static ReportDataset Create<T>(
        FilterDto filters,
        IEnumerable<T> rows,
        string? accountName = null,
        byte[]? logoImage = null)
    {
        var properties = typeof(T).GetProperties();
        var columns = new ReportColumn[properties.Length];
        for (var i = 0; i < properties.Length; i++)
        {
            columns[i] = new ReportColumn(properties[i].Name, properties[i].PropertyType);
        }

        var materialized = rows as ICollection<T> ?? [.. rows];
        var data = new List<object?[]>(materialized.Count);
        foreach (var row in materialized)
        {
            var values = new object?[properties.Length];
            for (var i = 0; i < properties.Length; i++)
            {
                values[i] = properties[i].GetValue(row);
            }
            data.Add(values);
        }

        return new ReportDataset
        {
            Title = filters.Name ?? string.Empty,
            FromDate = filters.DateTimeFilter1,
            ToDate = filters.DateTimeFilter2,
            GeneratedAt = DateTimeOffset.UtcNow,
            Columns = columns,
            Rows = data,
            AppliedFilters = BuildAppliedFilters(filters),
            AccountName = accountName,
            LogoImage = logoImage
        };
    }

    private static IReadOnlyList<KeyValuePair<string, string>> BuildAppliedFilters(FilterDto filters)
    {
        var applied = new List<KeyValuePair<string, string>>();

        void AddDate(string key, DateTimeOffset? value)
        {
            if (value.HasValue)
            {
                applied.Add(new KeyValuePair<string, string>(
                    key, value.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)));
            }
        }

        void AddText(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                applied.Add(new KeyValuePair<string, string>("Filter", value));
            }
        }

        void AddNumber(double? value)
        {
            if (value.HasValue)
            {
                applied.Add(new KeyValuePair<string, string>(
                    "Filter", value.Value.ToString(CultureInfo.InvariantCulture)));
            }
        }

        AddDate("FilterFrom", filters.DateTimeFilter1);
        AddDate("FilterTo", filters.DateTimeFilter2);
        AddDate("Filter", filters.DateTimeFilter3);
        AddText(filters.StringFilter1);
        AddText(filters.StringFilter2);
        AddText(filters.StringFilter3);
        AddNumber(filters.NumericFilter1);
        AddNumber(filters.NumericFilter2);
        AddNumber(filters.NumericFilter3);

        return applied;
    }
}
