using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.BankAccounts.Contracts;

namespace SpendingAnalyzer.Endpoints.BankAccounts;

public class GetBankAccountByIdEndpoint : EndpointWithoutRequest<BankAccountDetailResponse>
{
    private readonly SpendingAnalyzerDbContext _db;

    public GetBankAccountByIdEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/bankaccounts/{id}");
        AllowAnonymous();
        Description(q => q.WithTags("BankAccounts").Produces<BankAccountDetailResponse>(200).Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");

        var bankAccount = await _db.BankAccounts
            .Include(ba => ba.Bank)
            .Include(t => t.Transactions)
               .Where(t => t.Transactions.LastOrDefault() != null)
            .FirstOrDefaultAsync(ba => ba.Id == id, ct);

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