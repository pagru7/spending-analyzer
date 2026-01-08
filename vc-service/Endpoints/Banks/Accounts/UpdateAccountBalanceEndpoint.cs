using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Banks.Accounts.Contracts;
using SpendingAnalyzer.Entities;

namespace SpendingAnalyzer.Endpoints.Banks.Accounts;

public class UpdateAccountBalanceEndpoint : Endpoint<UpdateAccountBalanceRequest, BankAccountDetailResponse>
{
    private readonly SpendingAnalyzerDbContext _db;

    public UpdateAccountBalanceEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Post(ApiRoutes.BankAccountByIdBalance);
        AllowAnonymous();
        Description(q => q
            .WithTags("Accounts")
            .Produces<BankAccountDetailResponse>(200)
            .Produces(404));
    }

    public override async Task HandleAsync(UpdateAccountBalanceRequest req, CancellationToken ct)
    {
        var id = Route<int>("id");

        var bankAccount = await _db.Accounts
            .Include(ba => ba.Bank)
            .FirstOrDefaultAsync(ba => ba.Id == id, ct);

        if (bankAccount is null)
        {
            HttpContext.Response.StatusCode = 404;
            return;
        }

        // Get the latest transaction to determine current balance
        var lastTransaction = await _db.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == id)
            .OrderByDescending(t => t.Id)
            .FirstOrDefaultAsync(ct);

        var currentBalance = lastTransaction?.Balance ?? 0;
        var difference = req.NewBalance - currentBalance;

        // If no difference, just return current state
        if (difference == 0)
        {
            Response = new BankAccountDetailResponse
            {
                Id = bankAccount.Id,
                Name = bankAccount.Name,
                CreationDate = bankAccount.CreatedAt,
                Balance = currentBalance,
                IsInactive = bankAccount.IsInactive,
                BankId = bankAccount.BankId,
                BankName = bankAccount.Bank.Name
            };
            return;
        }

        // Create adjustment transaction
        var adjustmentTransaction = new Transaction
        {
            AccountId = id,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Amount = difference,
            Balance = req.NewBalance,
            Recipient = "System",
            Description = req.Description ?? "Balance Adjustment"
        };

        _db.Transactions.Add(adjustmentTransaction);
        await _db.SaveChangesAsync(ct);

        Response = new BankAccountDetailResponse
        {
            Id = bankAccount.Id,
            Name = bankAccount.Name,
            CreationDate = bankAccount.CreatedAt,
            Balance = req.NewBalance,
            IsInactive = bankAccount.IsInactive,
            BankId = bankAccount.BankId,
            BankName = bankAccount.Bank.Name
        };
    }
}
