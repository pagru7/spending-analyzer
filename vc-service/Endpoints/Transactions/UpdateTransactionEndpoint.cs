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

        var account = await _db.BankAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == req.AccountId, ct);

        if (account is null)
        {
            AddError("AccountId", "Account not found");
            ThrowIfAnyErrors();
            return;
        }

        using var transactionScope = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var transaction = await _db.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (transaction is null)
            {
                HttpContext.Response.StatusCode = 404;
                return;
            }

            transaction.Description = req.Description;
            transaction.AccountId = req.AccountId;
            transaction.Recipient = req.Recipient;
            var newAmount = req.IsIncome ? Math.Abs(req.Amount) : -Math.Abs(req.Amount);

            if (transaction.Amount != newAmount)
            {
                var previestTransaction = await _db.Transactions
                    .Where(t => t.AccountId == transaction.AccountId && t.Id < transaction.Id)
                    .OrderByDescending(t => t.Id)
                    .FirstAsync(ct);

                transaction.Amount = newAmount;
                transaction.Balance = previestTransaction.Balance + newAmount;

                var newestTransactions = await _db.Transactions
                    .Where(t => t.AccountId == transaction.AccountId && t.Id > transaction.Id)
                    .OrderBy(t => t.Id)
                    .ToArrayAsync(ct);

                var lastBalance = transaction.Balance;

                foreach (var nt in newestTransactions)
                {
                    nt.Balance = lastBalance + nt.Amount;
                    lastBalance = nt.Balance;
                }
            }

            await _db.SaveChangesAsync(ct);
            await transactionScope.CommitAsync(ct);

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
        catch
        {
            await transactionScope.RollbackAsync(ct);
            throw;
        }
    }
}