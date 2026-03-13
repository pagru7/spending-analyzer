using Spending_Analyzer_Mobile.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Spending_Analyzer_Mobile.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly DatabaseService _databaseService;

    public ApiService()
    {
        _httpClient = new HttpClient();
        _databaseService = DatabaseService.Instance;
    }

    private async Task<string> GetBaseUrlAsync()
    {
        var settings = await _databaseService.GetSettingsAsync();
        if (settings == null || string.IsNullOrWhiteSpace(settings.HostUrl))
        {
            throw new InvalidOperationException("Backend server URL is not configured.");
        }

        var host = settings.HostUrl.TrimEnd('/');
        if (!host.StartsWith("http://") && !host.StartsWith("https://"))
        {
            host = "http://" + host;
        }

        return $"{host}:{settings.Port}";
    }

    private async Task SetAuthHeaderAsync()
    {
        var settings = await _databaseService.GetSettingsAsync();
        if (settings != null && !string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("X-Account-Id", settings.AccountId);
        }
    }

    public async Task<SyncResult> SyncAllTransactionsAsync()
    {
        var transactions = await _databaseService.GetTransactionsAsync();
        return await SyncTransactionsAsync(transactions);
    }

    public async Task<SyncResult> SyncNewTransactionsAsync()
    {
        var transactions = await _databaseService.GetUnsynchronizedTransactionsAsync();
        return await SyncTransactionsAsync(transactions);
    }

    private async Task<SyncResult> SyncTransactionsAsync(List<Transaction> transactions)
    {
        var result = new SyncResult();

        try
        {
            var baseUrl = await GetBaseUrlAsync();
            await SetAuthHeaderAsync();

            var exportData = new ExportData
            {
                Transactions = transactions.Select(t => new TransactionDto
                {
                    LocalId = t.Id,
                    ServerId = t.ServerId,
                    Amount = t.Amount,
                    Recipient = t.Recipient,
                    Description = t.Description,
                    TransactionDate = t.TransactionDate,
                    TransactionType = t.TransactionType,
                    Balance = t.Balance
                }).ToList(),
                ExportDate = DateTime.Now
            };

            var json = JsonSerializer.Serialize(exportData, ApiSerializerContext.Default.ExportData);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{baseUrl}/api/transactions/sync", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var syncResponse = JsonSerializer.Deserialize(responseContent, ApiSerializerContext.Default.SyncResponse);

                if (syncResponse != null)
                {
                    // Mark sent transactions as synchronized
                    await _databaseService.MarkAsSynchronizedAsync(transactions.Select(t => t.Id));

                    // Update local data with server changes
                    foreach (var serverTransaction in syncResponse.UpdatedTransactions)
                    {
                        var localTransaction = transactions.FirstOrDefault(t => t.Id == serverTransaction.LocalId);
                        if (localTransaction != null)
                        {
                            localTransaction.ServerId = serverTransaction.ServerId;
                            localTransaction.Amount = serverTransaction.Amount;
                            localTransaction.Recipient = serverTransaction.Recipient;
                            localTransaction.Description = serverTransaction.Description;
                            localTransaction.TransactionDate = serverTransaction.TransactionDate;
                            localTransaction.TransactionType = serverTransaction.TransactionType;
                            localTransaction.Balance = serverTransaction.Balance;
                            localTransaction.IsSynchronized = true;
                            localTransaction.LastSyncDate = DateTime.Now;
                            await _databaseService.SaveTransactionAsync(localTransaction);
                        }
                    }

                    result.Success = true;
                    result.SyncedCount = transactions.Count;
                    result.Message = $"Successfully synchronized {transactions.Count} transaction(s).";
                }
            }
            else
            {
                result.Success = false;
                result.Message = $"Server returned error: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Sync failed: {ex.Message}";
        }

        return result;
    }
}

public class SyncResult
{
    public bool Success { get; set; }
    public int SyncedCount { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ExportData
{
    public List<TransactionDto> Transactions { get; set; } = [];
    public DateTime ExportDate { get; set; }
}

public class TransactionDto
{
    public int LocalId { get; set; }
    public string? ServerId { get; set; }
    public decimal Amount { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = TransactionTypes.Spending;
    public decimal Balance { get; set; }
}

public class SyncResponse
{
    public bool Success { get; set; }
    public List<TransactionDto> UpdatedTransactions { get; set; } = [];
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(ExportData))]
[JsonSerializable(typeof(SyncResponse))]
internal partial class ApiSerializerContext : JsonSerializerContext
{
}
