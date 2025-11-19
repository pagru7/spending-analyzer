using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;

namespace SpendingAnalyzer.Endpoints.BankAccounts;

public class SetBankAccountInactiveEndpoint : EndpointWithoutRequest
{
    private readonly SpendingAnalyzerDbContext _db;

    public SetBankAccountInactiveEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Delete("/api/bankaccounts/{id}");
        AllowAnonymous();
        Description(q => q.WithTags("BankAccounts").Produces(204).Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var bankAccount = await _db.BankAccounts.FirstOrDefaultAsync(ba => ba.Id == id, ct);

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




