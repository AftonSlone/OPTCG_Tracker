# OPTCG Tracker API

A modern .NET Web API for tracking One Piece Card Game match results with OAuth authentication.

## Features

- **OAuth Authentication**: Support for Google, GitHub, Microsoft, and Discord login
- **JWT Tokens**: Secure token-based authentication
- **Entity Framework Core**: Database-first approach with SQL Server
- **Layered Architecture**: Separated Core, Data, and API layers
- **RESTful API**: Clean endpoints for user management
- **React Frontend**: Modern React-based login and dashboard pages
- **Environment Variables**: Secure credential management via .env file

## Tech Stack

- **.NET 8** ASP.NET Core Web API
- **SQL Server LocalDB** with Entity Framework Core
- **JWT Authentication** for secure API access
- **OAuth 2.0** for third-party authentication
- **React** for frontend UI with React Router
- **Swagger/OpenAPI** for API documentation

## Project Structure

- **OPTCG.Tracker.API** - Web API layer with controllers and configuration
- **OPTCG.Tracker.Core** - Domain models and business logic services
- **OPTCG.Tracker.Data** - Database context and Entity Framework migrations
- **optcg-tracker-frontend** - React frontend application

## Prerequisites

- .NET 8 SDK
- SQL Server LocalDB (included with Visual Studio or available separately)
- Node.js and npm (for React frontend development)

## Setup Instructions

### 1. Clone and Build

```bash
git clone <repository-url>
cd OPTCG_Tracker
dotnet restore
dotnet build
```

### 2. Configure OAuth Providers

Create a `.env` file in the `OPTCG.Tracker.API` directory with your OAuth provider credentials:

```env
GOOGLE_CLIENT_ID=your-google-client-id
GOOGLE_CLIENT_SECRET=your-google-client-secret
GITHUB_CLIENT_ID=your-github-client-id
GITHUB_CLIENT_SECRET=your-github-client-secret
MICROSOFT_CLIENT_ID=your-microsoft-client-id
MICROSOFT_CLIENT_SECRET=your-microsoft-client-secret
DISCORD_CLIENT_ID=your-discord-client-id
DISCORD_CLIENT_SECRET=your-discord-client-secret
```

The `.env` file is already included in `.gitignore` to prevent committing sensitive credentials.

Alternatively, you can configure credentials in `appsettings.json` (not recommended for production):

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

### 4. Build React Frontend

The React app is built and served from the API's wwwroot folder. To build the React app:

```bash
cd optcg-tracker-frontend
npm install
npm run build
```

After building, copy the build output to the API's wwwroot folder:

```bash
Copy-Item -Path "build\*" -Destination "..\OPTCG.Tracker.API\wwwroot" -Recurse -Force
```

**Note:** The current setup has the React app already built and deployed to wwwroot. You only need to rebuild if you make changes to the React components.

### 5. Run the API

```bash
cd OPTCG.Tracker.API
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5126`
- Swagger UI: `http://localhost:5126/swagger`
- React Login Page: `http://localhost:5126/`

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
- OAuth client secrets should be stored in the `.env` file (already gitignored)
- HTTPS should be enforced in production
- Database connection strings should use environment variables in production
- The `.env` file is included in `.gitignore` to prevent committing sensitive credentials

## Future Enhancements

- Match tracking endpoints
- Deck management APIs
- Tournament organization features
- Statistics and analytics
- Real-time notifications

## License

This project is licensed under the MIT License.
