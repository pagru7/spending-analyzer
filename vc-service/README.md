# SpendingAnalyzer API

A .NET 10 Web API for managing banks, bank accounts, transactions, and transfers built with FastEndpoints, Entity Framework Core, PostgreSQL, and Serilog.

## ✅ Project Status: **COMPLETE & READY**

The application has been fully implemented according to the requirements specified in `requirements.md`. All endpoints are functional and the project compiles successfully.

## Features

- **Banks Management**: Create, read, update, and soft-delete (mark inactive) banks with optional bank accounts
- **Bank Accounts Management**: Create, read, update, and mark inactive bank accounts
- **Transactions Management**: Create, read, update, and delete transactions
- **Transfers Management**: Create, read, update, and delete transfers with **automatic balance updates**
- **Logging**: File-based logging using Serilog with daily rolling files
- **Database**: PostgreSQL with Entity Framework Core migrations
- **API Framework**: FastEndpoints 7.0 for high-performance endpoint definitions

## Technology Stack

- **.NET 10 RC** (latest version)
- **FastEndpoints 7.0.1** - Modern alternative to MVC controllers
- **Entity Framework Core 9.0.10** - ORM for database operations
- **Npgsql 9.0.4** - PostgreSQL provider
- **Serilog 9.0.0** - Structured logging to files
- **OpenAPI/Swagger** - API documentation (Development only)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (RC or later)
- [PostgreSQL 12+](https://www.postgresql.org/download/)
- Visual Studio Code, Visual Studio 2022, or JetBrains Rider

## Getting Started

### 1. Configure Database

Update the connection string in `appsettings.json` and `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=SpendingAnalyzer;Username=postgres;Password=your_password_here"
  }
}
```

### 2. Apply Database Migrations

```powershell
# The migration has already been created!
# Just update the database:
dotnet ef database update
```

### 3. Run the Application

```powershell
dotnet run
```

The API will be available at:

- **HTTP**: `http://localhost:5117`
- **HTTPS**: `https://localhost:7001`
- **OpenAPI**: `/openapi/v1.json` (Development mode only)

## API Endpoints

### Banks (`/api/banks`)

| Method   | Endpoint          | Description                                     |
| -------- | ----------------- | ----------------------------------------------- |
| `POST`   | `/api/banks`      | Create a new bank (with optional bank accounts) |
| `GET`    | `/api/banks`      | Get all banks with their accounts               |
| `GET`    | `/api/banks/{id}` | Get bank by ID with accounts                    |
| `PUT`    | `/api/banks/{id}` | Update bank name                                |
| `DELETE` | `/api/banks/{id}` | Mark bank as inactive (soft delete)             |

### Bank Accounts (`/api/Accounts`)

| Method   | Endpoint                 | Description                   |
| -------- | ------------------------ | ----------------------------- |
| `POST`   | `/api/Accounts`      | Create a new bank account     |
| `GET`    | `/api/Accounts`      | Get all bank accounts         |
| `GET`    | `/api/Accounts/{id}` | Get bank account by ID        |
| `PUT`    | `/api/Accounts/{id}` | Update bank account name      |
| `DELETE` | `/api/Accounts/{id}` | Mark bank account as inactive |

### Transactions (`/api/transactions`)

| Method   | Endpoint                 | Description                     |
| -------- | ------------------------ | ------------------------------- |
| `POST`   | `/api/transactions`      | Create a new transaction        |
| `GET`    | `/api/transactions`      | Get all transactions            |
| `GET`    | `/api/transactions/{id}` | Get transaction by ID           |
| `PUT`    | `/api/transactions/{id}` | Update transaction (all fields) |
| `DELETE` | `/api/transactions/{id}` | Delete transaction permanently  |

### Transfers (`/api/transfers`)

| Method   | Endpoint              | Description                                         |
| -------- | --------------------- | --------------------------------------------------- |
| `POST`   | `/api/transfers`      | Create transfer (updates balances)                  |
| `GET`    | `/api/transfers`      | Get all transfers                                   |
| `GET`    | `/api/transfers/{id}` | Get transfer by ID                                  |
| `PUT`    | `/api/transfers/{id}` | Update transfer (reverts old, applies new balances) |
| `DELETE` | `/api/transfers/{id}` | Delete transfer (reverts balance changes)           |

## Testing the API

Use the included `SpendingAnalyzer.http` file to test all endpoints. This file is compatible with:

- **Visual Studio Code** with REST Client extension
- **Visual Studio 2022** built-in HTTP file support
- **JetBrains Rider** HTTP Client

### Example: Create a Bank with Accounts

```http
POST http://localhost:5117/api/banks
Content-Type: application/json

{
  "name": "Chase Bank",
  "Accounts": [
    {
      "name": "Checking Account",
      "balance": 5000.00
    },
    {
      "name": "Savings Account",
      "balance": 10000.00
    }
  ]
}
```

## Logging

Logs are written to the `logs/` directory:

- **File pattern**: `spending-analyzer-yyyyMMdd.log`
- **Rolling interval**: Daily
- **Format**: `{Timestamp} [{Level}] {Message}{NewLine}{Exception}`

Example log entry:

```
2025-10-14 22:37:21.123 +00:00 [INF] Starting SpendingAnalyzer API
```

## Project Structure

```
SpendingAnalyzer/
├── Data/
│   └── SpendingAnalyzerDbContext.cs          # EF Core DbContext
├── Entities/
│   ├── Bank.cs                                # Bank entity
│   ├── BankAccount.cs                         # BankAccount entity
│   ├── Transaction.cs                         # Transaction entity
│   └── Transfer.cs                            # Transfer entity
├── Endpoints/
│   ├── Banks/
│   │   ├── Contracts/BankContracts.cs        # Request/Response DTOs
│   │   ├── CreateBankEndpoint.cs
│   │   ├── GetAllBanksEndpoint.cs
│   │   ├── GetBankByIdEndpoint.cs
│   │   ├── UpdateBankEndpoint.cs
│   │   └── DeleteBankEndpoint.cs
│   ├── Accounts/                          # Similar structure
│   ├── Transactions/                          # Similar structure
│   └── Transfers/                             # Similar structure
├── Migrations/                                # EF Core migrations
├── logs/                                      # Serilog output
├── Program.cs                                 # Application entry point
├── appsettings.json                           # Configuration
├── appsettings.Development.json               # Dev configuration
├── requirements.md                            # Business requirements
├── SpendingAnalyzer.http                      # API test file
└── README.md                                  # This file
```

## Business Rules Implementation

### 1. Banks

- ✅ Cannot be deleted, only marked as inactive (`IsInactive` flag)
- ✅ Name can be updated
- ✅ Can be created with a list of bank accounts in a single request

### 2. Bank Accounts

- ✅ Must be associated with a bank (foreign key required)
- ✅ Have creation date, name, and balance
- ✅ Can be marked as inactive (soft delete)
- ✅ Name can be updated

### 3. Transactions

- ✅ Must reference a valid bank account
- ✅ All fields (description, accountId, recipient, amount) can be updated
- ✅ Can be permanently deleted

### 4. Transfers

- ✅ **Automatically update account balances** on create
- ✅ Source and target accounts must be different
- ✅ When updated: old balance changes are reverted, new ones applied
- ✅ When deleted: balance changes are automatically reverted
- ✅ All fields can be updated (description, source, target, value)

## Development Notes

### FastEndpoints 7.0 API

This project uses FastEndpoints 7.0, which has a different API from earlier versions:

```csharp
// Setting response
Response = new MyResponse { ... };

// Returning 404
HttpContext.Response.StatusCode = 404;
return;

// Returning 204 No Content
HttpContext.Response.StatusCode = 204;

// Validation errors
AddError("fieldName", "Error message");
ThrowIfAnyErrors();
```

### Adding New Endpoints

1. Create a new class in the appropriate folder under `Endpoints/`
2. Inherit from `Endpoint<TRequest, TResponse>` or `EndpointWithoutRequest<TResponse>`
3. Implement `Configure()` and `HandleAsync()`

Example:

```csharp
public class MyEndpoint : Endpoint<MyRequest, MyResponse>
{
    public override void Configure()
    {
        Post("/api/myroute");
        AllowAnonymous();
    }

    public override async Task HandleAsync(MyRequest req, CancellationToken ct)
    {
        Response = new MyResponse { ... };
    }
}
```

### Database Migrations

```powershell
# Add a new migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

## Known Issues & Notes

- The project uses .NET 10 RC, which is a preview version
- EF Core tools (9.0.9) may show a warning about being older than runtime (9.0.10)
- These are expected and don't affect functionality

## Next Steps

1. **Update connection string** in appsettings files with your PostgreSQL credentials
2. **Run migrations**: `dotnet ef database update`
3. **Start the application**: `dotnet run`
4. **Test endpoints** using the `.http` file
5. **(Optional) Add authentication/authorization** as needed
6. **(Optional) Add input validation** with FluentValidation
7. **(Optional) Add unit tests** and integration tests

## License

This project was created for educational/demonstration purposes based on the requirements specified in `requirements.md`.
