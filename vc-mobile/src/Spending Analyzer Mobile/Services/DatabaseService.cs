using SQLite;
using Spending_Analyzer_Mobile.Models;

namespace Spending_Analyzer_Mobile.Services;

public class DatabaseService
{
    private readonly SQLiteAsyncConnection _database;
    private static DatabaseService? _instance;
    private static Task? _initializationTask;

    public static DatabaseService Instance => _instance ??= new DatabaseService();

    private DatabaseService()
    {
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "spending_analyzer.db");
        _database = new SQLiteAsyncConnection(dbPath);
    }

    public static async Task EnsureInitializedAsync()
    {
        if (_initializationTask == null)
        {
            _initializationTask = Instance.InitializeAsync();
        }
        await _initializationTask;
    }

    private async Task InitializeAsync()
    {
        await _database.CreateTableAsync<Transaction>();
        await _database.CreateTableAsync<AppSettings>();

        await TryAddTransactionColumnAsync("TransactionType", "TEXT");
        await TryAddTransactionColumnAsync("Balance", "REAL");

        var settings = await GetSettingsAsync();
        if (settings == null)
        {
            await SaveSettingsAsync(new AppSettings());
        }
    }

    private async Task TryAddTransactionColumnAsync(string columnName, string columnType)
    {
        try
        {
            await _database.ExecuteAsync($"ALTER TABLE Transaction ADD COLUMN {columnName} {columnType}");
        }
        catch (SQLiteException)
        {
            // Column already exists
        }
    }

    private static decimal GetSignedAmount(Transaction transaction)
    {
        return transaction.TransactionType switch
        {
            TransactionTypes.Income => transaction.Amount,
            TransactionTypes.Transfer => 0m,
            _ => -transaction.Amount
        };
    }

    private async Task RecalculateBalancesAsync()
    {
        var ordered = await _database.Table<Transaction>()
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.Id)
            .ToListAsync();

        var runningBalance = 0m;
        foreach (var transaction in ordered)
        {
            runningBalance += GetSignedAmount(transaction);
            transaction.Balance = runningBalance;
            await _database.UpdateAsync(transaction);
        }
    }

    // Transaction operations
    public async Task<List<Transaction>> GetTransactionsAsync()
    {
        return await _database.Table<Transaction>()
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetTransactionsAsync(int skip, int take)
    {
        return await _database.Table<Transaction>()
            .OrderByDescending(t => t.TransactionDate)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetFilteredTransactionsAsync(
        DateTime? startDate,
        DateTime? endDate,
        string? recipient,
        decimal? minAmount,
        decimal? maxAmount,
        int skip,
        int take)
    {
        var query = _database.Table<Transaction>();

        var allTransactions = await query.ToListAsync();

        var filtered = allTransactions.AsEnumerable();

        if (startDate.HasValue)
            filtered = filtered.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            filtered = filtered.Where(t => t.TransactionDate <= endDate.Value);

        if (!string.IsNullOrWhiteSpace(recipient))
            filtered = filtered.Where(t => t.Recipient.Contains(recipient, StringComparison.OrdinalIgnoreCase));

        if (minAmount.HasValue)
            filtered = filtered.Where(t => t.Amount >= minAmount.Value);

        if (maxAmount.HasValue)
            filtered = filtered.Where(t => t.Amount <= maxAmount.Value);

        return filtered
            .OrderByDescending(t => t.TransactionDate)
            .Skip(skip)
            .Take(take)
            .ToList();
    }

    public async Task<Transaction?> GetTransactionAsync(int id)
    {
        return await _database.Table<Transaction>()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<int> SaveTransactionAsync(Transaction transaction)
    {
        int result;
        if (transaction.Id != 0)
        {
            result = await _database.UpdateAsync(transaction);
        }
        else
        {
            result = await _database.InsertAsync(transaction);
        }

        await RecalculateBalancesAsync();
        return result;
    }

    public async Task<int> DeleteTransactionAsync(Transaction transaction)
    {
        var result = await _database.DeleteAsync(transaction);
        await RecalculateBalancesAsync();
        return result;
    }

    public async Task<List<Transaction>> GetUnsynchronizedTransactionsAsync()
    {
        return await _database.Table<Transaction>()
            .Where(t => !t.IsSynchronized)
            .ToListAsync();
    }

    public async Task MarkAsSynchronizedAsync(IEnumerable<int> transactionIds)
    {
        foreach (var id in transactionIds)
        {
            var transaction = await GetTransactionAsync(id);
            if (transaction != null)
            {
                transaction.IsSynchronized = true;
                transaction.LastSyncDate = DateTime.Now;
                await _database.UpdateAsync(transaction);
            }
        }
    }

    // Settings operations
    public async Task<AppSettings?> GetSettingsAsync()
    {
        return await _database.Table<AppSettings>()
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveSettingsAsync(AppSettings settings)
    {
        var existing = await GetSettingsAsync();
        if (existing != null)
        {
            settings.Id = existing.Id;
            return await _database.UpdateAsync(settings);
        }
        return await _database.InsertAsync(settings);
    }

    public async Task<int> GetTransactionCountAsync()
    {
        return await _database.Table<Transaction>().CountAsync();
    }

    public async Task<decimal> GetLatestBalanceAsync()
    {
        var latest = await _database.Table<Transaction>()
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.Id)
            .FirstOrDefaultAsync();

        return latest?.Balance ?? 0m;
    }

    public async Task<decimal> GetTotalSpendingAsync()
    {
        var transactions = await GetTransactionsAsync();
        return transactions
            .Where(t => t.TransactionType == TransactionTypes.Spending)
            .Sum(t => t.Amount);
    }
}
