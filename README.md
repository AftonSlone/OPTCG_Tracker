# OPTCG Tracker API

A modern .NET Web API for tracking One Piece Card Game match results with OAuth authentication.

## Features

- **OAuth Authentication**: Support for Google, Microsoft, Discord, and Twitch login
- **JWT Tokens**: Secure token-based authentication
- **Entity Framework Core**: Database-first approach with SQL Server
- **Layered Architecture**: Separated Core, Data, and API layers
- **RESTful API**: Clean endpoints for user management
- **React Frontend**: Modern React-based login and dashboard pages
- **Tailwind CSS**: Modern utility-first CSS framework for styling
- **Dark Mode**: User preference for light/dark theme with localStorage persistence
- **Slide-out Menu**: Navigation menu with placeholder links to future features
- **User Profile Management**: Display and edit user display name
- **Environment Variables**: Secure credential management via .env file

## Tech Stack

- **.NET 8** ASP.NET Core Web API
- **SQL Server LocalDB** with Entity Framework Core
- **JWT Authentication** for secure API access
- **OAuth 2.0** for third-party authentication
- **React** for frontend UI with React Router
- **Tailwind CSS** for modern utility-first styling
- **Swagger/OpenAPI** for API documentation

## Project Structure

- **OPTCG.Tracker.API** - Web API layer with controllers and configuration
- **OPTCG.Tracker.Core** - Domain models and business logic services
- **OPTCG.Tracker.Data** - Database context and Entity Framework migrations
- **optcg-tracker-frontend** - React frontend application with Tailwind CSS
  - `src/components` - React components (Login, Dashboard, Menu)
  - `tailwind.config.js` - Tailwind CSS configuration
  - `postcss.config.js` - PostCSS configuration for Tailwind

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
MICROSOFT_CLIENT_ID=your-microsoft-client-id
MICROSOFT_CLIENT_SECRET=your-microsoft-client-secret
DISCORD_CLIENT_ID=your-discord-client-id
DISCORD_CLIENT_SECRET=your-discord-client-secret
TWITCH_CLIENT_ID=your-twitch-client-id
TWITCH_CLIENT_SECRET=your-twitch-client-secret
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

The project uses **Tailwind CSS** for styling. Tailwind is already configured with:
- Custom color palette (purple/indigo theme)
- Dark mode support (class-based)
- PostCSS and Autoprefixer for browser compatibility

After building, copy the build output to the API's wwwroot folder:

```bash
Copy-Item -Path "build\*" -Destination "..\OPTCG.Tracker.API\wwwroot" -Recurse -Force
```

**Note:** The current setup has the React app already built and deployed to wwwroot with Tailwind CSS. You only need to rebuild if you make changes to the React components.

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
- `Microsoft`
- `Discord`
- `Twitch`

## Database Schema

### Users Table
- `Id` (int, primary key)
- `Email` (nvarchar, unique)
- `Username` (nvarchar, unique)
- `DisplayName` (nvarchar, nullable) - User's preferred display name
- `CreatedDate` (datetime)
- `LastModified` (datetime)
- `LastLoginDate` (datetime, nullable) - Tracks when user last logged in
- `Preferences` (nvarchar, nullable) - User preferences (JSON string)
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

## Testing

### Backend Tests
The project includes unit and integration tests for the .NET API using xUnit.

**Run backend tests:**
```bash
dotnet test OPTCG.Tracker.Tests\OPTCG.Tracker.Tests.csproj
```

**Test Coverage:**
- UserController tests (profile retrieval, profile updates)
- AuthController tests (OAuth login flow, username generation, logout)

**Testing Packages:**
- xUnit - Test framework
- Moq - Mocking framework
- Microsoft.EntityFrameworkCore.InMemory - In-memory database for testing
- Microsoft.AspNetCore.Mvc.Testing - Integration testing

### Frontend Tests
The React frontend includes component tests using React Testing Library.

**Run frontend tests:**
```bash
cd optcg-tracker-frontend
npm test
```

**Test Coverage:**
- Login component tests (renders correctly, OAuth links)
- Dashboard component tests (profile display, editing functionality)
- Menu component tests (navigation items, dark mode toggle)

**Testing Packages:**
- @testing-library/react - React component testing
- @testing-library/jest-dom - Jest DOM matchers
- @testing-library/user-event - User interaction simulation

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
