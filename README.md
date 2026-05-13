# OPTCG Tracker API

A modern .NET Web API for tracking One Piece Card Game match results with OAuth authentication.

## Features

- **OAuth Authentication**: Support for Google, GitHub, Microsoft, and Discord login
- **JWT Tokens**: Secure token-based authentication
- **Entity Framework Core**: Database-first approach with SQL Server
- **Docker Support**: Containerized application and database
- **RESTful API**: Clean endpoints for user management

## Tech Stack

- **.NET 8** ASP.NET Core Web API
- **SQL Server** with Entity Framework Core
- **JWT Authentication** for secure API access
- **Docker** & **Docker Compose** for containerization
- **OAuth 2.0** for third-party authentication

## Prerequisites

- .NET 8 SDK
- Docker Desktop (for containerized development)
- SQL Server (for local development) or use Docker

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

#### Option A: Docker (Recommended)
```bash
docker-compose up --build
```

#### Option B: Local SQL Server
1. Install SQL Server Express or LocalDB
2. Update connection string in `appsettings.json`
3. Run database migration:
```bash
dotnet ef database update
```

### 4. Run the API

#### With Docker:
```bash
docker-compose up
```

#### Locally:
```bash
cd OPTCG.Tracker.API
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- OpenAPI/Swagger: `https://localhost:5001/openapi`

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
# Create new migration
dotnet ef migrations add MigrationName

# Apply migration
dotnet ef database update
```

### Docker Development
```bash
# Build and run containers
docker-compose up --build

# View logs
docker-compose logs -f

# Stop containers
docker-compose down
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
