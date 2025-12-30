using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.BankAccounts.Contracts;

namespace SpendingAnalyzer.Endpoints.BankAccounts;

public class DeleteAccountEndpoint : EndpointWithoutRequest
{
    private readonly SpendingAnalyzerDbContext _db;

    public DeleteAccountEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Delete("/api/banks/{bankId}/{accountId}");
        AllowAnonymous();
        Description(q => q.WithTags("BankAccounts").Produces<BankAccountDetailResponse>(200).Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var bankId = Route<int>("bankId");
        var accountId = Route<int>("accountId");

        var bankAccount = await _db.BankAccounts
            .Include(ba => ba.Transactions)
            .FirstOrDefaultAsync(ba => ba.Id == accountId && ba.BankId == bankId, ct);

        if (bankAccount is null)
        {
            HttpContext.Response.StatusCode = 404;
            return;
        }

        if (bankAccount.Transactions.Count > 1)
        {
            await _db.Banks
                .ExecuteUpdateAsync(b => b.SetProperty(x => x.IsInactive, true), ct);           
        }
        else
        {
            _db.BankAccounts.Remove(bankAccount);
            await _db.SaveChangesAsync(ct);
        }

        Response = new BankAccountDetailResponse
        {
            Id = bankAccount.Id,
            Name = bankAccount.Name,
            CreationDate = bankAccount.CreatedAt,
            Balance = bankAccount.Transactions.LastOrDefault()?.Amount ?? 0,
            IsInactive = bankAccount.IsInactive,
            BankId = bankAccount.BankId,
        };
    }
}