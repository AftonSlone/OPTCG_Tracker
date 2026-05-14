# OPTCG Tracker API

A modern .NET Web API for tracking One Piece Card Game match results with OAuth authentication.

## Features

- **OAuth Authentication**: Support for Google, GitHub, Microsoft, and Discord login
- **JWT Tokens**: Secure token-based authentication
- **Entity Framework Core**: Database-first approach with SQL Server
- **Layered Architecture**: Separated Core, Data, and API layers
- **RESTful API**: Clean endpoints for user management

## Tech Stack

- **.NET 8** ASP.NET Core Web API
- **SQL Server LocalDB** with Entity Framework Core
- **JWT Authentication** for secure API access
- **OAuth 2.0** for third-party authentication
- **Swagger/OpenAPI** for API documentation

## Project Structure

- **OPTCG.Tracker.API** - Web API layer with controllers and configuration
- **OPTCG.Tracker.Core** - Domain models and business logic services
- **OPTCG.Tracker.Data** - Database context and Entity Framework migrations

## Prerequisites

- .NET 8 SDK
- SQL Server LocalDB (included with Visual Studio or available separately)

## Setup Instructions

### 1. Clone and Build

```bash
git clone <repository-url>
cd OPTCG_Tracker
dotnet restore
dotnet build
```

### 2. Configure OAuth Providers

Update `appsettings.json` with your OAuth provider credentials:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    },
    "GitHub": {
      "ClientId": "your-github-client-id", 
      "ClientSecret": "your-github-client-secret"
    },
    "Microsoft": {
      "ClientId": "your-microsoft-client-id",
      "ClientSecret": "your-microsoft-client-secret"
    },
    "Discord": {
      "ClientId": "your-discord-client-id",
      "ClientSecret": "your-discord-client-secret"
    }
  }
}
```

### 3. Database Setup

The project uses SQL Server LocalDB by default. To set up the database:

1. Ensure SQL Server LocalDB is installed
2. Run database migration:
```bash
cd OPTCG.Tracker.API
dotnet ef database update
```

The connection string in `appsettings.json` is configured for LocalDB:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OPTCGTracker;Trusted_Connection=true;MultipleActiveResultSets=true"
}
```

### 4. Run the API

```bash
cd OPTCG.Tracker.API
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5126`
- Swagger UI: `http://localhost:5126/swagger`

## API Endpoints

### Authentication
- `GET /api/auth/login/{provider}` - Initiate OAuth login
- `GET /api/auth/callback` - OAuth callback handler
- `POST /api/auth/logout` - Logout user

### User Management
- `GET /api/user/profile` - Get current user profile
- `PUT /api/user/profile` - Update user profile

### OAuth Providers
Supported providers for `{provider}` parameter:
- `Google`
- `GitHub` 
- `Microsoft`
- `Discord`

## Database Schema

### Users Table
- `Id` (int, primary key)
- `Email` (nvarchar, unique)
- `Username` (nvarchar, unique)
- `CreatedDate` (datetime)
- `LastModified` (datetime)
- `OAuthProvider` (nvarchar)
- `OAuthProviderUserId` (nvarchar)

## Development

### Entity Framework Migrations
```bash
# Create new migration (run from OPTCG.Tracker.API directory)
cd OPTCG.Tracker.API
dotnet ef migrations add MigrationName --project ../OPTCG.Tracker.Data

# Apply migration
dotnet ef database update --project ../OPTCG.Tracker.Data
```

## Security Notes

- JWT secret key should be changed in production
- OAuth client secrets should be stored securely
- HTTPS should be enforced in production
- Database connection strings should use environment variables in production

## Future Enhancements

- Match tracking endpoints
- Deck management APIs
- Tournament organization features
- Statistics and analytics
- Real-time notifications

## License

This project is licensed under the MIT License.
