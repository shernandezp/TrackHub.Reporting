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

using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models.Manager;

namespace TrackHub.Reporting.Infrastructure.GraphQLApi;

public class GpsManagerReader(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager)), IGpsManagerReader
{
    public async Task<IReadOnlyCollection<ManagerOperatorVm>> GetOperatorsAsync(CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = @"
                query {
                    operatorsByCurrentAccount {
                        operatorId
                        name
                        enabled
                    }
                }"
        };
        var data = await QueryAsync<List<ManagerOperatorVm>>(request, cancellationToken);
        return data;
    }
    public async Task<IReadOnlyCollection<ManagerDeviceVm>> GetSynchronizedDevicesAsync(Guid accountId, string? detectedStatus, Guid? operatorId, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = @"
                query($accountId: UUID!, $detectedStatus: DetectedStatus, $operatorId: UUID) {
                    synchronizedDevices(query: { accountId: $accountId, detectedStatus: $detectedStatus, operatorId: $operatorId }) {
                        deviceId
                        accountId
                        operatorId
                        serial
                        name
                        identifier
                        providerDisplayName
                        providerStatus
                        detectedStatus
                        firstSeenAt
                        lastSeenAt
                        lastAssignedAt
                        ignoredAt
                    }
                }",
            Variables = new
            {
                accountId,
                detectedStatus,
                operatorId
            }
        };
        return await QueryAsync<List<ManagerDeviceVm>>(request, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ManagerDeviceVm>> GetUnassignedDevicesAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = @"
                query($accountId: UUID!) {
                    unassignedSynchronizedDevices(query: { accountId: $accountId }) {
                        deviceId
                        accountId
                        operatorId
                        serial
                        name
                        identifier
                        providerDisplayName
                        providerStatus
                        detectedStatus
                        firstSeenAt
                        lastSeenAt
                        lastAssignedAt
                        ignoredAt
                    }
                }",
            Variables = new { accountId }
        };
        return await QueryAsync<List<ManagerDeviceVm>>(request, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ManagerTransporterDeviceAssignmentVm>> GetAssignmentsByAccountAsync(Guid accountId, bool activeOnly, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = @"
                query($accountId: UUID!, $activeOnly: Boolean!) {
                    transporterDeviceAssignmentsByAccount(query: { accountId: $accountId, activeOnly: $activeOnly }) {
                        transporterDeviceAssignmentId
                        accountId
                        transporterId
                        deviceId
                        effectiveFrom
                        effectiveTo
                        priority
                        isPrimary
                        status
                        assignmentReason
                    }
                }",
            Variables = new { accountId, activeOnly }
        };
        return await QueryAsync<List<ManagerTransporterDeviceAssignmentVm>>(request, cancellationToken);
    }
}
