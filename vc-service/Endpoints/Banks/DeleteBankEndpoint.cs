using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;

namespace SpendingAnalyzer.Endpoints.Banks;

public class DeleteBankEndpoint : EndpointWithoutRequest
{
    private readonly SpendingAnalyzerDbContext _db;

    public DeleteBankEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Delete(ApiRoutes.BankById);
        AllowAnonymous();
        Description(q => q.WithTags("Banks").Produces(204).Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");

        var bank = await _db.Banks.FirstOrDefaultAsync(b => b.Id == id, ct);

        if (bank is null)
        {
            HttpContext.Response.StatusCode = 404;
            return;
        }

        bank.IsInactive = true;
        await _db.SaveChangesAsync(ct);

        HttpContext.Response.StatusCode = 204;
    }
}




