using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Banks.Accounts.Contracts;

namespace SpendingAnalyzer.Endpoints.Banks.Accounts;

public class GetBankAccountByIdEndpoint : EndpointWithoutRequest<BankAccountDetailResponse>
{
    private readonly SpendingAnalyzerDbContext _db;

    public GetBankAccountByIdEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get(ApiRoutes.BankAccountById);
        AllowAnonymous();
        Description(q => q.WithTags("Accounts").Produces<BankAccountDetailResponse>(200).Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var bankId = Route<int>("bankId");
        var accountId = Route<int>("accountId");

        var bankAccount = await _db.Accounts
            .Include(ba => ba.Bank)
            .Include(t => t.Transactions)
               .Where(t => t.Transactions.LastOrDefault() != null)
            .FirstOrDefaultAsync(ba => ba.Id == accountId && ba.BankId == bankId, ct);

        if (bankAccount is null)
        {
            HttpContext.Response.StatusCode = 404;
            return;
        }

        Response = new BankAccountDetailResponse
        {
            Id = bankAccount.Id,
            Name = bankAccount.Name,
            CreationDate = bankAccount.CreatedAt,
            Balance = bankAccount.Transactions.LastOrDefault()!.Balance,
            IsInactive = bankAccount.IsInactive,
            BankId = bankAccount.BankId,
            BankName = bankAccount.Bank.Name
        };
    }
}