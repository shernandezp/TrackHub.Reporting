using TrackHub.Reporting.Domain.Exceptions;
using TrackHub.Reporting.Domain.Interfaces.Foundation;

namespace TrackHub.Reporting.Infrastructure.GraphQLApi;

public class PlatformFeatureReader(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager)), IPlatformFeatureReader
{
    public async Task EnsureFeatureEnabledAsync(Guid accountId, string featureKey, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = @"
                query($accountId: UUID!, $featureKey: String!) {
                    validateFeatureEnabled(accountId: $accountId, featureKey: $featureKey)
                }",
            Variables = new
            {
                accountId,
                featureKey
            }
        };

        var enabled = await QueryAsync<bool>(request, cancellationToken);
        if (!enabled)
        {
            throw new FeatureDisabledException(featureKey);
        }
    }
}

public class ReportAuditWriter(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager)), IReportAuditWriter
{
    public async Task RecordReportExportAsync(
        Guid accountId,
        string actorType,
        string actorId,
        string reportCode,
        string filtersJson,
        int rowCount,
        string format,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = @"
                mutation(
                    $accountId: UUID!,
                    $actorType: String!,
                    $actorId: String!,
                    $reportCode: String!,
                    $newValuesJson: String!,
                    $correlationId: String
                ) {
                    createAuditEvent(command: { auditEvent: {
                        accountId: $accountId,
                        actorType: $actorType,
                        actorId: $actorId,
                        action: ""ReportExported"",
                        resourceType: ""Report"",
                        resourceId: $reportCode,
                        result: ""Success"",
                        oldValuesJson: null,
                        newValuesJson: $newValuesJson,
                        reason: null,
                        ipAddress: null,
                        userAgent: null,
                        correlationId: $correlationId
                    }}) {
                        auditEventId
                    }
                }",
            Variables = new
            {
                accountId,
                actorType,
                actorId,
                reportCode,
                newValuesJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    reportCode,
                    filters = filtersJson,
                    rowCount,
                    format
                }),
                correlationId
            }
        };

        await MutationAsync<object>(request, cancellationToken);
    }
}
