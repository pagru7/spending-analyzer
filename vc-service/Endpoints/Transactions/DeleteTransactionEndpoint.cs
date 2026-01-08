using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;

namespace SpendingAnalyzer.Endpoints.Transactions;

public class DeleteTransactionEndpoint : EndpointWithoutRequest
{
    private readonly SpendingAnalyzerDbContext _db;

    public DeleteTransactionEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Delete(ApiRoutes.TransactionById);
        AllowAnonymous();
        Description(q => q.WithTags("Transactions").Produces(204).Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");

        var transaction = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == id, ct);

        if (transaction is null)
        {
            HttpContext.Response.StatusCode = 404;
            return;
        }

        _db.Transactions.Remove(transaction);
        await _db.SaveChangesAsync(ct);

        HttpContext.Response.StatusCode = 204;
    }
}




