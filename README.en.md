# TrackHub Reporting API

## Key Features

- **Excel Report Generation**: Export operational data in Excel format for easy analysis
- **Live Position Reports**: Real-time listing of all transporters with their current locations
- **Historical Position Records**: Query transporter position history within specified time ranges
- **Geofence Reports**: Identify transporters currently within defined geographic boundaries
- **Group-Based Filtering**: Reports automatically filtered by user's assigned groups
- **REST API Interface**: Simple, straightforward endpoints for report retrieval
- **Scalable Architecture**: Designed to integrate additional report types and data sources

---

## Quick Start

### Prerequisites

- .NET 10.0 SDK
- PostgreSQL 14+
- TrackHub Authority Server running (for authentication)
- TrackHub Manager and Router APIs (for data access)

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/shernandezp/TrackHub.Reporting.git
   cd TrackHub.Reporting
   ```

2. **Configure the connections** in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "ManagerConnection": "Host=localhost;Database=trackhub_manager;Username=postgres;Password=yourpassword"
     },
     "GraphQL": {
       "RouterEndpoint": "https://localhost:5001/graphql"
     }
   }
   ```

3. **Start the application**:
   ```bash
   dotnet run --project src/Web
   ```

4. **Access the API documentation** at `https://localhost:5001/swagger`

### Example API Call

```bash
# Get live report for all transporters
curl -X GET "https://localhost:5001/api/reports/live" \
  -H "Authorization: Bearer {your_token}" \
  -o live_report.xlsx
```

---

## Components and Resources

| Component                | Description                                           | Documentation                                                                 |
|--------------------------|-------------------------------------------------------|-------------------------------------------------------------------------------|
| .NET Core                | Development platform for modern applications          | [.NET Core Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview) |

---

## Overview

## Key Features

The Reporting API for TrackHub is a REST API that provides an interface for TrackHub Web to access reporting data. It is designed to be scalable and flexible, allowing the integration of additional reports from different sources within the system. Reports are generated in Excel format.

## Available Reports

- **LiveReport**: Lists transporters (units) along with their current location. It is filtered based on the groups assigned to the user.
- **PositionRecord**: Provides a record of transporter (unit) positions within a specified period.
- **TransportersInGeofence**: Identifies transporters (units) that are currently within a geofence.

## License

This project is licensed under the Apache 2.0 License. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.