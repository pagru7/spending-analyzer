using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Transactions.Contracts;
using SpendingAnalyzer.Entities;

namespace SpendingAnalyzer.Endpoints.Transactions;

public class CreateTransactionEndpoint : Endpoint<CreateTransactionRequest, TransactionResponse>
{
    private readonly SpendingAnalyzerDbContext _db;

    public CreateTransactionEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Post("/api/transactions");
        AllowAnonymous();
        Description(q => q.WithTags("Transactions").Produces<TransactionResponse>(201).Accepts<CreateTransactionRequest>("application/json"));
    }

    public override async Task HandleAsync(CreateTransactionRequest req, CancellationToken ct)
    {
        var account = await _db.BankAccounts.FirstOrDefaultAsync(a => a.Id == req.AccountId, ct);

        if (account is null)
        {
            AddError("AccountId", "Account not found");
            ThrowIfAnyErrors();
            return;
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Description = req.Description,
            AccountId = req.AccountId,
            Recipient = req.Recipient,
            Amount = req.Amount
        };

        account.Balance += req.Amount;

        _db.Transactions.Add(transaction);
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




