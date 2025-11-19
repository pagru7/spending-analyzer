using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Transactions.Contracts;

namespace SpendingAnalyzer.Endpoints.Transactions;

public class GetAllTransactionsEndpoint : EndpointWithoutRequest<List<TransactionResponse>>
{
    private readonly SpendingAnalyzerDbContext _db;

    public GetAllTransactionsEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/transactions");
        AllowAnonymous();
        Description(q => q.WithTags("Transactions").Produces<List<TransactionResponse>>(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var transactions = await _db.Transactions
            .Include(t => t.Account)
            .ToListAsync(ct);

        Response = transactions.Select(t => new TransactionResponse
        {
            Id = t.Id,
            Description = t.Description,
            AccountId = t.AccountId,
            AccountName = t.Account.Name,
            Recipient = t.Recipient,
            Amount = t.Amount
        }).ToList();
    }
}




