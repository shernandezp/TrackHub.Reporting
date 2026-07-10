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

using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Infrastructure.GraphQLApi;

public class AdminReportReader(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager)), IAdminReportReader
{
    internal const string AccountsQuery = @"
                query {
                    accounts {
                        accountId
                        name
                        status
                        statusId
                        typeId
                        active
                        lastModified
                    }
                }";

    internal const string AccountFeaturesMasterQuery = @"
                query($accountId: UUID!) {
                    accountFeaturesMaster(query: { accountId: $accountId }) {
                        featureKey
                        enabled
                        tier
                    }
                }";

    internal const string GroupsByAccountQuery = @"
                query {
                    groupsByAccount {
                        groupId
                        name
                        active
                        accountId
                    }
                }";

    internal const string UsersByGroupQuery = @"
                query($groupId: Long!) {
                    usersByGroup(query: { groupId: $groupId }) {
                        userId
                        username
                    }
                }";

    internal const string TransportersByGroupQuery = @"
                query($groupId: Long!) {
                    transportersByGroup(query: { groupId: $groupId }) {
                        transporterId
                        name
                    }
                }";

    public async Task<IReadOnlyCollection<AdminAccountVm>> GetAccountsAsync(CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest { Query = AccountsQuery };
        return await QueryAsync<List<AdminAccountVm>>(request, cancellationToken);
    }

    public async Task<IReadOnlyCollection<AdminFeatureVm>> GetAccountFeaturesAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest { Query = AccountFeaturesMasterQuery, Variables = new { accountId } };
        return await QueryAsync<List<AdminFeatureVm>>(request, cancellationToken);
    }

    public async Task<IReadOnlyCollection<AdminGroupVm>> GetGroupsByAccountAsync(CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest { Query = GroupsByAccountQuery };
        return await QueryAsync<List<AdminGroupVm>>(request, cancellationToken);
    }

    public async Task<IReadOnlyCollection<AdminUserVm>> GetUsersByGroupAsync(long groupId, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest { Query = UsersByGroupQuery, Variables = new { groupId } };
        return await QueryAsync<List<AdminUserVm>>(request, cancellationToken);
    }

    public async Task<IReadOnlyCollection<AdminTransporterVm>> GetTransportersByGroupAsync(long groupId, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest { Query = TransportersByGroupQuery, Variables = new { groupId } };
        return await QueryAsync<List<AdminTransporterVm>>(request, cancellationToken);
    }
}
