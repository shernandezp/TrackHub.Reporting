## Components and Resources

| Component                | Description                                           | Documentation                                                                 |
|--------------------------|-------------------------------------------------------|-------------------------------------------------------------------------------|
| .NET Core                | Development platform for modern applications          | [.NET Core Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview) |

# Reporting API for TrackHub

## Key Features

The Reporting API for TrackHub is a REST API that provides an interface for TrackHub Web to access reporting data. It is designed to be scalable and flexible, allowing the integration of additional reports from different sources within the system. Reports are generated in Excel format.

## Available Reports

- **LiveReport**: Lists transporters (units) along with their current location. It is filtered based on the groups assigned to the user.
- **PositionRecord**: Provides a record of transporter (unit) positions within a specified period.
- **TransportersInGeofence**: Identifies transporters (units) that are currently within a geofence.

## License

This project is licensed under the Apache 2.0 License. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.