using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Banks.Accounts.Contracts;
using SpendingAnalyzer.Entities;

namespace SpendingAnalyzer.Endpoints.Banks.Accounts;

public class CreateAccountEndpoint : Endpoint<CreateBankAccountRequest, BankAccountDetailResponse>
{
    private readonly SpendingAnalyzerDbContext _db;

    public CreateAccountEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Post(ApiRoutes.BankAccounts);
        AllowAnonymous();
        Description(q => q.WithTags("Accounts").Produces<BankAccountDetailResponse>(201));
    }

    public override async Task HandleAsync(CreateBankAccountRequest req, CancellationToken ct)
    {
        var bank = await _db.Banks.FirstOrDefaultAsync(b => b.Id == req.BankId, ct);

        if (bank is null)
        {
            AddError("BankId", "Bank not found");
            ThrowIfAnyErrors();
            return;
        }

        var bankAccount = req.ToAccount();
        var initialTransaction = new Transaction()
        {
            Account = bankAccount,
            Amount = req.Balance,
            Balance = req.Balance,
            CreatedAt = DateTime.UtcNow,
            Description = $"Initial balance for account {bankAccount.Name}",
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Recipient = "Initial Balance"
        };

        bankAccount.Transactions.Add(initialTransaction);

        _db.Accounts.Add(bankAccount);
        await _db.SaveChangesAsync(ct);

        Response = new BankAccountDetailResponse
        {
            Id = bankAccount.Id,
            Name = bankAccount.Name,
            CreationDate = bankAccount.CreatedAt,
            Balance = initialTransaction.Balance,
            IsInactive = bankAccount.IsInactive,
            BankId = bank.Id,
            BankName = bank.Name
        };
    }
}