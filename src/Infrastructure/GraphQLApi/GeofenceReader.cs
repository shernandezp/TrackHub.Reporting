using TrackHub.Reporting.Domain.Interfaces.Geofence;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Infrastructure.GraphQLApi;

public class GeofenceReader(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Geofence)), IGeofenceReader
{
    // Single source of truth for the queries this reader sends; the
    // ServiceContracts tests validate these exact strings against the Geofence schema.
    internal const string TransportersInGeofenceQuery = @"
                query {
                    transportersInGeofence {
                      transporterName
                      transporterId
                      geofenceName
                      geofenceId
                    }
              }";

    internal const string GeofenceEventsQuery = @"
                query($from: DateTime!, $to: DateTime!, $transporterId: UUID) {
                    geofenceEvents(query: {from: $from, to: $to, transporterId: $transporterId}) {
                        transporterName
                        geofenceName
                        datetimeIn
                        datetimeOut
                        totalTime
                        latitude
                        longitude
                    }
                }";

    /// <summary>
    /// Retrieves the device positions asynchronously
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TransporterInGeofenceVm>> GetTransportersInGeofenceAsync(CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = TransportersInGeofenceQuery
        };
        return await QueryAsync<IEnumerable<TransporterInGeofenceVm>>(request, cancellationToken);

    }

    /// <summary>
    /// Retrieves geofence events asynchronously filtered by date range and optional transporter
    /// </summary>
    public async Task<IEnumerable<GeofenceEventReportVm>> GetGeofenceEventsAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = GeofenceEventsQuery,
            Variables = new
            {
                from = filters.DateTimeFilter1,
                to = filters.DateTimeFilter2,
                transporterId = string.IsNullOrEmpty(filters.StringFilter1) ? null : filters.StringFilter1
            }
        };
        return await QueryAsync<IEnumerable<GeofenceEventReportVm>>(request, cancellationToken);
        
    }
}

