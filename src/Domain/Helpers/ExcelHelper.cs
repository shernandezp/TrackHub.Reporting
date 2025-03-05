// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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
using System.Resources;
using ClosedXML.Excel;
using Common.Domain.Extensions;
using TrackHub.Reporting.Domain.Interfaces.Helpers;

namespace TrackHub.Reporting.Domain.Helpers;

/// <summary>
/// Helper class for exporting data to Excel format.
/// </summary>
public sealed class ExcelHelper : IExcelHelper
{
    /// <summary>
    /// Exports the given data to an Excel file.
    /// </summary>
    /// <typeparam name="T">The type of the data to export.</typeparam>
    /// <param name="title">The title of the report.</param>
    /// <param name="fromDate">The start date of the report.</param>
    /// <param name="toDate">The end date of the report.</param>
    /// <param name="data">The data to export.</param>
    /// <returns>A memory stream containing the Excel file.</returns>
    public byte[] Export<T>(string title, DateTimeOffset? fromDate, DateTimeOffset? toDate, IEnumerable<T> data, CultureInfo culture)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Report");
        worksheet.Cell("A1").Value = GetDateLabel(fromDate, toDate, title);
        worksheet.Cell("A1").Style.Font.SetBold(true);
        worksheet.Cell("A1").Style.Font.FontSize = 16;

        var table = worksheet.Cell("A2").InsertTable(data.AsEnumerable());

        var properties = typeof(T).GetProperties().ToList();

        var resourceManager = new ResourceManager(typeof(Resources.Resources));

        for (int colNumber = 1; colNumber <= properties.Count; colNumber++)
        {
            var property = properties[colNumber - 1];
            SetColumnFormat(worksheet, colNumber, property.PropertyType, property.Name);

            // Set headers
            var headerCell = table.Column(colNumber).FirstCell();
            var displayName = resourceManager.GetString(property.Name, culture) ?? property.Name;
            headerCell.SetValue(displayName);
        }

        worksheet.Range(1, 1, 1, table.ColumnCount()).Merge();
        table.Theme = XLTableTheme.TableStyleMedium9;
        worksheet.Columns().AdjustToContents();

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }

    private static readonly string[] CoordinatesFields = ["Latitude", "Longitude"];

    /// <summary>
    /// Sets the format of a column based on the property type.
    /// </summary>
    /// <param name="worksheet">The worksheet containing the column.</param>
    /// <param name="colNumber">The column number.</param>
    /// <param name="propertyType">The type of the property.</param>
    private static void SetColumnFormat(IXLWorksheet worksheet, int colNumber, Type propertyType, string propertyName)
    {
        switch (propertyType)
        {
            case Type t when t == typeof(DateTime) || t == typeof(DateTime?):
                worksheet.Column(colNumber).Style.DateFormat.Format = "yyyy-MM-dd HH:mm";
                break;
            case Type t when t == typeof(double) || t == typeof(decimal):
                worksheet.Column(colNumber).Style.NumberFormat.Format
                    = CoordinatesFields.Contains(propertyName) ? "0.00000" : "0.00";
                break;
            case Type t when t == typeof(int):
                worksheet.Column(colNumber).Style.NumberFormat.Format = "#";
                break;
            case Type t when t == typeof(bool):
                worksheet.Column(colNumber).Style.NumberFormat.Format = "@";
                break;
            case Type t when t == typeof(string):
                worksheet.Column(colNumber).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                break;
            default:
                // do nothing
                break;
        }
    }

    /// <summary>
    /// Generates a label for the date range of the report.
    /// </summary>
    /// <param name="fromDate">The start date of the report.</param>
    /// <param name="toDate">The end date of the report.</param>
    /// <param name="title">The title of the report.</param>
    /// <returns>A string representing the date range label.</returns>
    private static string GetDateLabel(DateTimeOffset? fromDate, DateTimeOffset? toDate, string title)
        => toDate != null
            ? $"{title} - ({fromDate.FormatDateTime()} - {toDate.FormatDateTime()})"
            : fromDate != null ? $"{title} - ({fromDate.Value.DateTime.FormatDate()})" : title;
}
