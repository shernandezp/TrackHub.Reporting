using TrackHub.Reporting.Domain.Interfaces.Router;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Infrastructure.GraphQLApi;

public class RouterReader(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Router)), IRouterReader
{

    /// <summary>
    /// Retrieves the device positions asynchronously
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<PositionVm>> GetDevicePositionsAsync(CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = @"
            query {
                devicePositionsByUser {
                    attributes {
                        temperature
                        satellites
                        mileage
                        ignition
                        hobbsMeter
                    }
                    altitude
                    address
                    deviceName
                    transporterType
                    state
                    speed
                    serverDateTime
                    longitude
                    latitude
                    eventId
                    transporterId
                    deviceDateTime
                    course
                    country
                    city
                }
            }"
        };
        return await QueryAsync<IEnumerable<PositionVm>>(request, cancellationToken);
        
    }

    public async Task<IEnumerable<PositionVm>> GetPositionsRecordAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = @"
                query($transporterId: UUID!, $to: DateTime!, $from: DateTime!) {
                    positionsByTransporter(query: { transporterId: $transporterId, to: $to, from: $from }) {
                        attributes {
                            temperature
                            satellites
                            mileage
                            ignition
                            hobbsMeter
                        }
                        altitude
                        address
                        deviceName
                        transporterType
                        state
                        speed
                        serverDateTime
                        longitude
                        latitude
                        eventId
                        transporterId
                        deviceDateTime
                        course
                        country
                        city
                    }
                }",
            Variables = new
            {
                transporterId = filters.StringFilter1,
                from = filters.DateTimeFilter1,
                to = filters.DateTimeFilter2
            }
        };
        return await QueryAsync<IEnumerable<PositionVm>>(request, cancellationToken);
    }
}
