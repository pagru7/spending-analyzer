using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Transactions.Contracts;

namespace SpendingAnalyzer.Endpoints.Transactions;

public class GetAllImportedTransactionsEndpoint : EndpointWithoutRequest<List<ImportedTransactionResponse>>
{
    private readonly SpendingAnalyzerDbContext _db;
    private readonly ILogger<GetAllImportedTransactionsEndpoint> _logger;

    public GetAllImportedTransactionsEndpoint(SpendingAnalyzerDbContext db, ILogger<GetAllImportedTransactionsEndpoint> logger)
    {
        _db = db;
        _logger = logger;
    }

    public override void Configure()
    {
        Get(ApiRoutes.ImportedTransactions);
        AllowAnonymous();
        Description(q => q.WithTags("Transactions").Produces<List<ImportedTransactionResponse>>(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Fetching all imported transactions.");
            var transactions = await _db.ImportedTransactions
                .Include(t => t.Account)
                .OrderByDescending(t => t.Id)
                .Select(t => new ImportedTransactionResponse
                {
                    Id = t.Id,
                    ExternalId = t.ExternalId,
                    ExternalIdParsed = t.ExternalIdParsed,
                    IssueDate = t.IssueDate,
                    Type = t.Type,
                    Amount = t.Amount,
                    Currency = t.Currency,
                    Balance = t.Balance,
                    IssuerBankAccountNumber = t.IssuerBankAccountNumber,
                    IssuerName = t.IssuerName,
                    Description = t.Description,
                    Description2 = t.Description2,
                    AccountId = t.AccountId,
                    AccountName = t.Account.Name,
                }).ToListAsync(ct);


            Response = transactions;
            _logger.LogInformation("Returning {TransactionCount} imported transactions.", transactions.Count);
        }
        catch (OperationCanceledException ex) when (ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Request was canceled by the caller.");
            if (!HttpContext.Response.HasStarted)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
        }
    }
}
