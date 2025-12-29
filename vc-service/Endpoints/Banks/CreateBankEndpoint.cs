using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Banks.Contracts;
using SpendingAnalyzer.Entities;

namespace SpendingAnalyzer.Endpoints.Banks;

public class CreateBankEndpoint : Endpoint<CreateBankRequest, BankResponse>
{
    private readonly SpendingAnalyzerDbContext _db;

    public CreateBankEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Post("/api/banks");
        AllowAnonymous();
        Description(q => q.WithTags("Banks").Produces<BankResponse>(201));
    }

    public override async Task HandleAsync(CreateBankRequest req, CancellationToken ct)
    {
        var bank = req.ToBank();

        _db.Banks.Add(bank);
        await _db.SaveChangesAsync(ct);

        Response = new BankResponse
        {
            Id = bank.Id,
            Name = bank.Name,
            IsInactive = bank.IsInactive,
            BankAccounts = bank.Accounts.Select(a => new BankAccountResponse
            {
                Id = a.Id,
                Name = a.Name,
                CreationDate = a.CreatedAt,
                Balance = req.BankAccounts!.First(ba => ba.Name == a.Name).Balance,
                IsInactive = a.IsInactive
            }).ToArray()
        };
    }
}