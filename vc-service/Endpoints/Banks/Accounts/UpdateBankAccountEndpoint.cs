using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Banks.Accounts.Contracts;

namespace SpendingAnalyzer.Endpoints.Banks.Accounts;

public class UpdateBankAccountEndpoint : Endpoint<UpdateBankAccountRequest, BankAccountDetailResponse>
{
    private readonly SpendingAnalyzerDbContext _db;

    public UpdateBankAccountEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Put(ApiRoutes.BankAccountById);
        AllowAnonymous();
        Description(q => q.WithTags("Accounts").Produces<BankAccountDetailResponse>(200).Produces(404));
    }

    public override async Task HandleAsync(UpdateBankAccountRequest req, CancellationToken ct)
    {
        var id = Route<int>("id");

        var bankAccount = await _db.Accounts
            .Include(ba => ba.Bank)
            .Include(t=>t.Transactions)
            .Where(t => t.Transactions.LastOrDefault() != null)
            .FirstOrDefaultAsync(ba => ba.Id == id, ct);

        if (bankAccount is null)
        {
            HttpContext.Response.StatusCode = 404;
            return;
        }

        bankAccount.Name = req.Name;
        await _db.SaveChangesAsync(ct);

        Response = new BankAccountDetailResponse
        {
            Id = bankAccount.Id,
            Name = bankAccount.Name,
            CreationDate = bankAccount.CreatedAt,
            Balance = bankAccount.Transactions.LastOrDefault()?.Amount ?? 0,
            IsInactive = bankAccount.IsInactive,
            BankId = bankAccount.BankId,
            BankName = bankAccount.Bank.Name
        };
    }
}




