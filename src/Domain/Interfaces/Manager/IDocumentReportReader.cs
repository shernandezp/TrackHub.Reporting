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

using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Domain.Interfaces.Manager;

// Reads the Manager document data the reports project (spec 04 §13). Each call runs under the caller's
// propagated token; the Manager query enforces owner-visibility + classification and the `documents`
// feature gate. All list reads page transparently up to the 100k report row limit.
public interface IDocumentReportReader
{
    // Pre-checks the `documents` billing feature so a disabled account fails with FEATURE_DISABLED (403)
    // rather than a raw GraphQL error.
    Task EnsureDocumentsFeatureAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ReportDocumentVm>> GetExpiringDocumentsAsync(int withinDays, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ReportDocumentTypeVm>> GetDocumentTypesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AdminGroupVm>> GetGroupsByAccountAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AdminTransporterVm>> GetTransportersByGroupAsync(long groupId, CancellationToken cancellationToken);

    // Returns null when the owner is not visible to the caller (so the caller's account enumeration stays
    // group-scoped — non-visible owners are excluded rather than crashing the report). AC13.
    Task<IReadOnlyCollection<ReportDocumentVm>?> GetDocumentsForOwnerAsync(string ownerEntityType, string ownerEntityId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ReportShareVm>> GetDocumentSharesByAccountAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ReportDocumentVm>> SearchDocumentsAsync(DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken);
}
