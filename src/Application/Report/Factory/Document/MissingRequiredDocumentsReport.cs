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
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Helpers;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Document;

// Missing-required-documents report (spec 04 §13, AC13). For each group-visible transporter, lists the
// required document types (enabled + required) that lack an Active document. Owner enumeration and the
// per-owner document read are both group-scoped by the Manager queries.
public sealed class MissingRequiredDocumentsReport(IDocumentReportReader reader, IExcelHelper helper) : IReport
{
    private const string OwnerTypeTransporter = "Transporter";

    public string ReportCode => DocumentReportCodes.MissingRequiredDocuments;

    public async Task<ReportExportResult> GenerateAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        await reader.EnsureDocumentsFeatureAsync(cancellationToken);

        var requiredCategories = (await reader.GetDocumentTypesAsync(cancellationToken))
            .Where(t => t.Required && t.Enabled)
            .Select(t => t.Category)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var rows = new List<MissingRequiredDocumentRowVm>();
        if (requiredCategories.Count == 0)
        {
            return Export(filters, rows);
        }

        // Enumerate group-visible transporters (a transporter may sit in several groups → dedupe by id).
        var transporters = new Dictionary<Guid, string>();
        foreach (var group in await reader.GetGroupsByAccountAsync(cancellationToken))
        {
            foreach (var transporter in await reader.GetTransportersByGroupAsync(group.GroupId, cancellationToken))
            {
                transporters[transporter.TransporterId] = transporter.Name;
            }
        }

        foreach (var (transporterId, name) in transporters.OrderBy(t => t.Value, StringComparer.OrdinalIgnoreCase))
        {
            var documents = await reader.GetDocumentsForOwnerAsync(OwnerTypeTransporter, transporterId.ToString(), cancellationToken);
            if (documents is null)
            {
                continue; // owner not visible to the caller → excluded (group-scoping, AC13)
            }

            var activeCategories = documents
                .Where(d => string.Equals(d.Status, "Active", StringComparison.OrdinalIgnoreCase))
                .Select(d => d.Category)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var category in requiredCategories.Where(c => !activeCategories.Contains(c)).OrderBy(c => c, StringComparer.OrdinalIgnoreCase))
            {
                rows.Add(new MissingRequiredDocumentRowVm(OwnerTypeTransporter, name, transporterId.ToString(), category));
            }
        }

        return Export(filters, rows);
    }

    private ReportExportResult Export(FilterDto filters, List<MissingRequiredDocumentRowVm> rows)
    {
        var culture = new CultureInfo(filters.Language);
        var bytes = helper.Export(filters.Name, filters.DateTimeFilter1, filters.DateTimeFilter2, rows, culture);
        return new ReportExportResult(bytes, rows.Count);
    }
}
