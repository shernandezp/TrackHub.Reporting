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

using HotChocolate;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Infrastructure.GraphQLApi;

public class DocumentReportReader(IGraphQLClientFactory graphQLClient, IUser user, IAccountFeatureReader featureReader)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager)), IDocumentReportReader
{
    // Manager clamps take to 500, so reports page through until exhausted or the report row limit.
    private const int PageSize = 500;
    private const int MaxRows = 100_000;

    private const string DocumentFields = "category ownerEntityType ownerEntityId fileName classification status expiresAt";

    internal const string ExpiringDocumentsQuery = @"
                query($withinDays: Int!, $skip: Int!, $take: Int!) {
                    expiringDocuments(query: { withinDays: $withinDays, skip: $skip, take: $take }) {
                        category ownerEntityType ownerEntityId fileName classification status expiresAt
                    }
                }";

    internal const string DocumentTypesQuery = @"
                query($accountId: UUID!) {
                    documentTypes(query: { accountId: $accountId, includeDisabled: false }) {
                        category required enabled
                    }
                }";

    internal const string GroupsByAccountQuery = @"
                query {
                    groupsByAccount { groupId name active accountId }
                }";

    internal const string TransportersByGroupQuery = @"
                query($groupId: Long!) {
                    transportersByGroup(query: { groupId: $groupId }) { transporterId name }
                }";

    internal const string DocumentsForOwnerQuery = @"
                query($accountId: UUID!, $ownerEntityType: String!, $ownerEntityId: String!, $skip: Int!, $take: Int!) {
                    documentsForOwner(query: { accountId: $accountId, ownerEntityType: $ownerEntityType, ownerEntityId: $ownerEntityId, skip: $skip, take: $take }) {
                        category ownerEntityType ownerEntityId fileName classification status expiresAt
                    }
                }";

    internal const string SharesByAccountQuery = @"
                query($accountId: UUID!, $skip: Int!, $take: Int!) {
                    publicLinkGrantsByAccount(query: { accountId: $accountId, skip: $skip, take: $take }) {
                        resourceType resourceId scopes purpose expiresAt revokedAt accessCount lastAccessedAt
                    }
                }";

    internal const string SearchDocumentsQuery = @"
                query($from: DateTime, $to: DateTime, $skip: Int!, $take: Int!) {
                    searchDocuments(query: { filter: { from: $from, to: $to }, skip: $skip, take: $take }) {
                        category ownerEntityType ownerEntityId fileName classification status expiresAt
                    }
                }";

    private Guid AccountId => user.AccountId ?? throw new UnauthorizedAccessException();

    public Task EnsureDocumentsFeatureAsync(CancellationToken cancellationToken)
        => featureReader.EnsureFeatureEnabledAsync(AccountId, FeatureKeys.Documents, cancellationToken);

    public Task<IReadOnlyCollection<ReportDocumentVm>> GetExpiringDocumentsAsync(int withinDays, CancellationToken cancellationToken)
        => FetchAllAsync<ReportDocumentVm>((skip, take) => new GraphQLRequest
        {
            Query = ExpiringDocumentsQuery,
            Variables = new { withinDays, skip, take }
        }, cancellationToken);

    public async Task<IReadOnlyCollection<ReportDocumentTypeVm>> GetDocumentTypesAsync(CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest { Query = DocumentTypesQuery, Variables = new { accountId = AccountId } };
        return await QueryAsync<List<ReportDocumentTypeVm>>(request, cancellationToken);
    }

    public async Task<IReadOnlyCollection<AdminGroupVm>> GetGroupsByAccountAsync(CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest { Query = GroupsByAccountQuery };
        return await QueryAsync<List<AdminGroupVm>>(request, cancellationToken);
    }

    public async Task<IReadOnlyCollection<AdminTransporterVm>> GetTransportersByGroupAsync(long groupId, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest { Query = TransportersByGroupQuery, Variables = new { groupId } };
        return await QueryAsync<List<AdminTransporterVm>>(request, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ReportDocumentVm>?> GetDocumentsForOwnerAsync(string ownerEntityType, string ownerEntityId, CancellationToken cancellationToken)
    {
        try
        {
            return await FetchAllAsync<ReportDocumentVm>((skip, take) => new GraphQLRequest
            {
                Query = DocumentsForOwnerQuery,
                Variables = new { accountId = AccountId, ownerEntityType, ownerEntityId, skip, take }
            }, cancellationToken);
        }
        catch (GraphQLException)
        {
            // Manager returned a GraphQL error (FORBIDDEN when the owner is outside the caller's
            // visibility) — exclude this owner from the group-scoped report rather than failing it.
            return null;
        }
    }

    public Task<IReadOnlyCollection<ReportShareVm>> GetDocumentSharesByAccountAsync(CancellationToken cancellationToken)
        => FetchAllAsync<ReportShareVm>((skip, take) => new GraphQLRequest
        {
            Query = SharesByAccountQuery,
            Variables = new { accountId = AccountId, skip, take }
        }, cancellationToken);

    public Task<IReadOnlyCollection<ReportDocumentVm>> SearchDocumentsAsync(DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken)
        => FetchAllAsync<ReportDocumentVm>((skip, take) => new GraphQLRequest
        {
            Query = SearchDocumentsQuery,
            Variables = new { from, to, skip, take }
        }, cancellationToken);

    private async Task<IReadOnlyCollection<T>> FetchAllAsync<T>(Func<int, int, GraphQLRequest> buildRequest, CancellationToken cancellationToken)
    {
        var all = new List<T>();
        var skip = 0;
        // Fetch one page BEYOND the report limit so an over-limit result set reaches ExcelHelper, which
        // then fails clearly (ReportLimitExceededException → 400) rather than silently truncating (AC12).
        while (all.Count <= MaxRows)
        {
            var page = await QueryAsync<List<T>>(buildRequest(skip, PageSize), cancellationToken);
            if (page.Count == 0)
            {
                break;
            }

            all.AddRange(page);
            if (page.Count < PageSize)
            {
                break;
            }

            skip += PageSize;
        }

        return all;
    }
}
