using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Transactions.Contracts;

namespace SpendingAnalyzer.Endpoints.Transactions;

public class GetAllTransactionsEndpoint : EndpointWithoutRequest<List<TransactionResponse>>
{
    private readonly SpendingAnalyzerDbContext _db;
    private readonly ILogger<GetAllTransactionsEndpoint> _logger;

    public GetAllTransactionsEndpoint(SpendingAnalyzerDbContext db, ILogger<GetAllTransactionsEndpoint> logger)
    {
        _db = db;
        _logger = logger;
    }

    public override void Configure()
    {
        Get(ApiRoutes.Transactions);
        AllowAnonymous();
        Description(q => q.WithTags("Transactions").Produces<List<TransactionResponse>>(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Fetching all transactions.");
            var transactions = await _db.Transactions
                .Include(t => t.Account)
                .OrderByDescending(t=>t.Id)
                .ToArrayAsync(ct);
            
            _logger.LogInformation("Fetched {TransactionCount} transactions from database.", transactions.Length);

            var response = transactions.Select(t => new TransactionResponse
            {
                Id = t.Id,
                Description = t.Description,
                AccountId = t.AccountId,
                AccountName = t.Account.Name,
                Recipient = t.Recipient,
                Amount = t.Amount,
                TransactionDate = t.TransactionDate,
            }).ToList();

            Response = response;
            _logger.LogInformation("Returning {TransactionCount} transactions.", response.Count);
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




