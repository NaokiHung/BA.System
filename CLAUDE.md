# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Budget Assistant (BA.System) is a full-stack expense tracking application with a .NET 8 Web API backend and Angular 19 frontend. The system uses JWT authentication and dual SQLite databases (UserDb.db for users, ExpenseDb.db for expenses).

## Architecture

### Backend (.NET 8 Web API)
- **Layered Architecture**: API → Business → Data → Core
- **Dual Database Strategy**: Separate contexts for Users and Expenses
- **JWT Authentication**: Token-based with custom claims including DisplayName
- **Extension-based Configuration**: Modular service registration in `/Extensions`

Key directories:
- `BA.Server.API/`: Controllers and extensions
- `BA.Server.Business/`: Service layer
- `BA.Server.Core/`: DTOs, entities, interfaces, enums
- `BA.Server.Data/`: DbContexts and repositories

### Frontend (Angular 19)
- **Standalone Components**: No NgModules, uses routes-based feature modules
- **Angular Material UI**: Primary UI framework
- **Feature-based Structure**: Organized by domain (auth, expense, user, dashboard)
- **JWT Interceptor**: Automatic token attachment
- **Guard-protected Routes**: Auth and no-auth guards

## Development Commands

### Backend (.NET)
```bash
# Run server (from BA.Server directory)
dotnet run

# Create migration
dotnet ef migrations add <MigrationName> --context UserDbContext
dotnet ef migrations add <MigrationName> --context ExpenseDbContext

# Update database
dotnet ef database update --context UserDbContext
dotnet ef database update --context ExpenseDbContext

# Build
dotnet build

# Clean
dotnet clean
```

### Frontend (Angular)
```bash
# Development server (from budget-assistant-web directory)
npm start
# or
ng serve

# Build
ng build

# Test
ng test

# Generate component
ng generate component <component-name>
```

## Database Architecture

- **UserDbContext**: Handles user authentication and profiles
- **ExpenseDbContext**: Manages expenses and budgets with composite keys
- **Connection Strings**: Configured in appsettings.json for both databases
- **Migrations**: Separate migration folders for each context

## Authentication Flow

1. JWT tokens contain custom claims: `nameid`, `unique_name`, `DisplayName`
2. Frontend AuthService parses tokens and maps claims to User model
3. AuthInterceptor automatically attaches Bearer tokens
4. Guards protect routes based on authentication state

## Key Patterns

- **Extension Methods**: Service registration grouped by concern
- **Repository Pattern**: Generic base repository with specific implementations  
- **DTO Mapping**: Request/Response DTOs separate from entities
- **Environment Configuration**: Separate settings for dev/prod
- **Error Handling**: Centralized in controllers with proper HTTP status codes

## Important Notes

- Backend uses traditional Chinese comments
- Expense entities support both cash and credit card types
- Monthly budgets use composite primary keys (UserId, Year, Month)
- Frontend uses TypeScript strict mode
- Authentication tokens expire in 24 hours (configurable)