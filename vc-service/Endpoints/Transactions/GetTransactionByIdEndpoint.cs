using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Transactions.Contracts;

namespace SpendingAnalyzer.Endpoints.Transactions;

public class GetTransactionByIdEndpoint : EndpointWithoutRequest<TransactionResponse>
{
    private readonly SpendingAnalyzerDbContext _db;

    public GetTransactionByIdEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get(ApiRoutes.TransactionById);
        AllowAnonymous();
        Description(q => q.WithTags("Transactions").Produces<TransactionResponse>(200).Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");

        var transaction = await _db.Transactions
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (transaction is null)
        {
            HttpContext.Response.StatusCode = 404;
            return;
        }

        Response = new TransactionResponse
        {
            Id = transaction.Id,
            Description = transaction.Description,
            AccountId = transaction.AccountId,
            AccountName = transaction.Account.Name,
            Recipient = transaction.Recipient,
            Amount = transaction.Amount
        };
    }
}




