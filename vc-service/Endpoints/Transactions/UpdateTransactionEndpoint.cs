using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Transactions.Contracts;

namespace SpendingAnalyzer.Endpoints.Transactions;

public class UpdateTransactionEndpoint : Endpoint<UpdateTransactionRequest, TransactionResponse>
{
    private readonly SpendingAnalyzerDbContext _db;

    public UpdateTransactionEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Put("/api/transactions/{id}");
        AllowAnonymous();
        Description(q => q.WithTags("Transactions").Produces<TransactionResponse>(200).Produces(404));
    }

    public override async Task HandleAsync(UpdateTransactionRequest req, CancellationToken ct)
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

        var account = await _db.BankAccounts.FirstOrDefaultAsync(a => a.Id == req.AccountId, ct);

        if (account is null)
        {
            AddError("AccountId", "Account not found");
            ThrowIfAnyErrors();
            return;
        }

        transaction.Description = req.Description;
        transaction.AccountId = req.AccountId;
        transaction.Recipient = req.Recipient;
        transaction.Amount = req.Amount;

        await _db.SaveChangesAsync(ct);

        Response = new TransactionResponse
        {
            Id = transaction.Id,
            Description = transaction.Description,
            AccountId = transaction.AccountId,
            AccountName = account.Name,
            Recipient = transaction.Recipient,
            Amount = transaction.Amount
        };
    }
}




