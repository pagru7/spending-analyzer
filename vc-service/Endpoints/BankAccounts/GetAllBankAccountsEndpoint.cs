using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.BankAccounts.Contracts;

namespace SpendingAnalyzer.Endpoints.BankAccounts;

public class GetAllBankAccountsEndpoint
    : EndpointWithoutRequest<BankAccountDetailResponse[]>
{
    private readonly SpendingAnalyzerDbContext _db;

    public GetAllBankAccountsEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/bankaccounts");
        AllowAnonymous();
        Description(q => q.WithTags("BankAccounts")
            .Produces<BankAccountDetailResponse[]>(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var bankAccounts = await _db.BankAccounts
            .Include(ba => ba.Bank)
            .Include(t => t.Transactions.LastOrDefault())
            .ToListAsync(ct);

        Response = bankAccounts.Select(ba => new BankAccountDetailResponse
        {
            Id = ba.Id,
            Name = ba.Name,
            CreationDate = ba.CreatedAt,
            Balance = ba.Transactions.LastOrDefault()!.Balance,
            IsInactive = ba.IsInactive,
            BankId = ba.BankId,
            BankName = ba.Bank.Name
        }).ToArray();
    }
}