namespace TrackHub.Reporting.Domain.Interfaces.Geofence;

public interface IGeofenceReader
{
    Task<IEnumerable<TransporterInGeofenceVm>> GetTransportersInGeofenceAsync(CancellationToken cancellationToken);
}
