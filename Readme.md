# Spending Analyzer

Monorepo for a personal finance system with three main applications: backend API, web client, and Android mobile client. The repository also includes Docker orchestration and API contract artifacts for local development.

## Projects

### 1. `vc-service` (Backend API)

Main backend built with .NET and FastEndpoints.

Main features:

- Manage banks and accounts (create, update, list, deactivate).
- Manage transactions.
- Handle transfers with automatic balance updates.
- PostgreSQL persistence via Entity Framework Core migrations.
- Structured file logging with Serilog.
- OpenAPI/Swagger support in development.

### 2. `vc-client/sa-client` (Web App)

Frontend single-page app built with React + TypeScript + Vite.

Main features:

- User interface for interacting with Spending Analyzer data.
- Modular component-based frontend structure.
- Fast local development with Vite.
- Typed codebase and linting setup for maintainability.
- Container-ready setup (`Dockerfile`, local compose file).

### 3. `vc-mobile` (Android App)

Mobile client for Spending Analyzer built with .NET for Android.

Main features:

- Dashboard with spending summary.
- Add transaction flow.
- Transaction history view.
- Settings screen.
- Local persistence for mobile-side data.

### 4. `vc-client/copilot` (Support Artifacts)

Project support folder containing API contract and assistant context files.

Main features:

- `swagger.json` for API schema/contract sharing.
- `app.md` for app-specific notes/context.

## Repository-Level Files

- `docker-compose.yml`: Root orchestration entry point for local multi-service setup.
- `feature-request.md`: Product/feature notes.
- `git.ignored/db_backup/`: Local backup storage (ignored from source control).

## Quick Start (High Level)

1. Start the API (`vc-service`) and database.
2. Start the web app (`vc-client/sa-client`) for browser access.
3. Run `vc-mobile` from Visual Studio/.NET Android tooling for mobile testing.

For detailed setup and commands, see each project-specific `README.md`.
