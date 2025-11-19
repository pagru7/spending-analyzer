using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.BankAccounts.Contracts;
using SpendingAnalyzer.Entities;

namespace SpendingAnalyzer.Endpoints.BankAccounts;

public class CreateBankAccountEndpoint : Endpoint<CreateBankAccountRequest, BankAccountDetailResponse>
{
    private readonly SpendingAnalyzerDbContext _db;

    public CreateBankAccountEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Post("/api/bankaccounts");
        AllowAnonymous();
        Description(q => q.WithTags("BankAccounts").Produces<BankAccountDetailResponse>(201));
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

        var bankAccount = new BankAccount
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Balance = req.Balance,
            CreationDate = DateTime.UtcNow,
            IsInactive = false,
            BankId = req.BankId
        };

        _db.BankAccounts.Add(bankAccount);
        await _db.SaveChangesAsync(ct);

        Response = new BankAccountDetailResponse
        {
            Id = bankAccount.Id,
            Name = bankAccount.Name,
            CreationDate = bankAccount.CreationDate,
            Balance = bankAccount.Balance,
            IsInactive = bankAccount.IsInactive,
            BankId = bank.Id,
            BankName = bank.Name
        };
    }
}




