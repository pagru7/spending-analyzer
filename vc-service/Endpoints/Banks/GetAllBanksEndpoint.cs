using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Banks.Contracts;

namespace SpendingAnalyzer.Endpoints.Banks;

public class GetAllBanksEndpoint : EndpointWithoutRequest<List<BankResponse>>
{
    private readonly SpendingAnalyzerDbContext _db;

    public GetAllBanksEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/banks");
        AllowAnonymous();
        Description(q => q.WithTags("Banks").Produces<List<BankResponse>>(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var banks = await _db.Banks
            .Include(b => b.BankAccounts)
            .ToListAsync(ct);

        Response = banks.Select(bank => new BankResponse
        {
            Id = bank.Id,
            Name = bank.Name,
            IsInactive = bank.IsInactive,
            BankAccounts = bank.BankAccounts.Select(a => new BankAccountResponse
            {
                Id = a.Id,
                Name = a.Name,
                CreationDate = a.CreationDate,
                Balance = a.Balance,
                IsInactive = a.IsInactive
            }).ToList()
        }).ToList();
    }
}




