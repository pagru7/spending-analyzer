using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Banks.Accounts.Contracts;

namespace SpendingAnalyzer.Endpoints.Banks.Accounts;

public class GetAllAccountsEndpoint
    : EndpointWithoutRequest<BankAccountDetailResponse[]>
{
    private readonly SpendingAnalyzerDbContext _db;

    public GetAllAccountsEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get(ApiRoutes.Accounts);
        AllowAnonymous();
        Description(q => q.WithTags("Accounts")
            .Produces<BankAccountDetailResponse[]>(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var Accounts = await _db.Accounts
            .Include(ba => ba.Bank)
            .Include(t => t.Transactions.LastOrDefault())
            .ToListAsync(ct);

        Response = Accounts.Select(ba => new BankAccountDetailResponse
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