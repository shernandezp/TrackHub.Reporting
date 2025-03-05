using TrackHub.Reporting.Domain.Interfaces.Geofence;

namespace TrackHub.Reporting.Infrastructure.GraphQLApi;

public class GeofenceReader(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Geofence)), IGeofenceReader
{

    /// <summary>
    /// Retrieves the device positions asynchronously
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TransporterInGeofenceVm>> GetTransportersInGeofenceAsync(CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = @"
                query {
                    transportersInGeofence {
                      transporterName
                      transporterId
                      geofenceName
                      geofenceId
                    }
              }"
        };
        return await QueryAsync<IEnumerable<TransporterInGeofenceVm>>(request, cancellationToken);

    }
}
