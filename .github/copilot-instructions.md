# Spending Analyzer

## Build, test, and lint commands

| Area | Commands | Notes |
| --- | --- | --- |
| Web client (`vc-client\sa-client`) | `pnpm dev`  `pnpm lint`  `pnpm build` | Uses pnpm (`pnpm-lock.yaml` is checked in). There is no automated test script yet. |
| API (`vc-service`) | `docker compose up`  `dotnet ef database update`  `dotnet run`  `dotnet build SpendingAnalyzer.sln` | `docker-compose.yml` here starts PostgreSQL only; `docker-compose.full.yml` runs PostgreSQL + API. `SpendingAnalyzer.http` is the checked-in way to exercise endpoints manually. If `dotnet build` fails with MSB3021/MSB3027, a running `SpendingAnalyzer.exe` or Visual Studio instance is locking `bin\Debug`. |
| Mobile (`vc-mobile\src`) | `dotnet build SAM.slnx` | Android app solution lives under `vc-mobile\src`. There is no automated test or lint command checked in. |

## High-level architecture

- This is a monorepo with three runnable apps: `vc-service` (ASP.NET Core API), `vc-client\sa-client` (React/Vite SPA), and `vc-mobile` (native Android app on .NET). The root `docker-compose.yml` is deprecated; use the compose files inside the project folders instead.
- `vc-service` is the backend for banks, accounts, transactions, and imported transactions. `Program.cs` wires FastEndpoints, EF Core/PostgreSQL, permissive CORS, Serilog file logging, and Swagger/OpenAPI in Development. Route constants live in `vc-service\Endpoints\Routes.cs`, entities in `Entities`, EF configuration in `Data\SpendingAnalyzerDbContext.cs`, and request/response DTOs sit beside endpoint folders in `Contracts`.
- The imported-transactions flow spans multiple API areas: `Banks\Accounts\ImportTransactionsEndpoint.cs` accepts CSV uploads under a bank-account route, `Services\TransactionImportProcessor.cs` normalizes file encoding (UTF-8 with CP1250 fallback), and `Services\InteligoTransactionImportDataParser.cs` maps CSV columns into `ImportedTransaction` rows. Dedup is enforced per account through `ExternalIdParsed`.
- `vc-client\sa-client` is a single-page app that talks directly to the API. `src\main.tsx` configures one shared Axios instance for `axios-hooks` with `VITE_API_URL` and a fallback of `http://localhost:5117`. `src\App.tsx` is the shell that swaps between views and dialogs, `src\hooks\useAppData.ts` centralizes the main bank/transaction fetch-refetch cycle, `src\types\api.ts` is the shared DTO source, `src\features` holds app-specific screens/dialogs, and `src\components\ui` contains shadcn/ui primitives behind the `@` alias.
- `vc-client\copilot\swagger.json` and `vc-client\copilot\app.md` are repo-local support artifacts for frontend work. Use them as context for available API shapes and intended UX before inventing new frontend structures.
- `vc-mobile` is not just another UI for the same runtime path. It persists data locally with SQLite in `Services\DatabaseService.cs`, keeps connection details in `Models\AppSettings.cs`, and syncs/export transactions through `Services\ApiService.cs`. That mobile sync path posts to `/api/transactions/sync`, which is not implemented in the current `vc-service` endpoint tree, so verify the target backend before changing either side.

## Key conventions

- Prefer the per-project compose files over the root compose file. The root file is intentionally a pointer, not the main local-dev entry point.
- Add API endpoints under the existing feature folders and reuse `ApiRoutes` constants instead of hardcoding route strings in endpoint classes.
- Banks and accounts use soft delete via `IsInactive`; existing UI and endpoints preserve records and mark them inactive instead of physically removing them.
- Account routes are bank-scoped (`/api/banks/{bankId}/accounts/...`), and the web client mirrors that nested route shape in its request URLs.
- Frontend data access is standardized on `axios-hooks` plus a single shared Axios client from `src\main.tsx`. For new client-side API work, extend `src\types\api.ts` and follow the `useAppData`/manual execute pattern instead of adding ad-hoc fetch helpers.
- Frontend UI work should stay aligned with the current stack noted in `vc-client\copilot\app.md`: shadcn/ui components, `react-hook-form`, and Axios/`axios-hooks`.
- Repository docs are partially stale. The root and service READMEs mention transfer support and a "complete" API, but the current `vc-service\Endpoints` tree only covers banks, accounts, transactions, imported transactions, and a test endpoint. Check source before assuming a documented feature already exists.
