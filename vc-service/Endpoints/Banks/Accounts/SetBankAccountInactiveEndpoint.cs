using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;

namespace SpendingAnalyzer.Endpoints.Banks.Accounts;

public class SetBankAccountInactiveEndpoint : EndpointWithoutRequest
{
    private readonly SpendingAnalyzerDbContext _db;

    public SetBankAccountInactiveEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Delete(ApiRoutes.BankAccountByIdActive);
        AllowAnonymous();
        Description(q => q.WithTags("Accounts").Produces(204).Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var bankId = Route<int>("bankId");
        var accountId = Route<int>("accountId");

        var bankAccount = await _db.Accounts
            .FirstOrDefaultAsync(ba => ba.Id == accountId && ba.BankId == bankId, ct);

        if (bankAccount is null)
        {
            HttpContext.Response.StatusCode = 404;
            return;
        }

        bankAccount.IsInactive = true;
        await _db.SaveChangesAsync(ct);

        HttpContext.Response.StatusCode = 204;
    }
}




