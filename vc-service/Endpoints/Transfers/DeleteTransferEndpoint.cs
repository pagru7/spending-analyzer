using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;

namespace SpendingAnalyzer.Endpoints.Transfers;

public class DeleteTransferEndpoint : EndpointWithoutRequest
{
    private readonly SpendingAnalyzerDbContext _db;

    public DeleteTransferEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Delete("/api/transfers/{id}");
        AllowAnonymous();
        Description(q => q.WithTags("Transfers").Produces(204).Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var transfer = await _db.Transfers
            .Include(t => t.SourceAccount)
            .Include(t => t.TargetAccount)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (transfer is null)
        {
            HttpContext.Response.StatusCode = 404;
            return;
        }

        // Revert balance changes
        transfer.SourceAccount.Balance += transfer.Value;
        transfer.TargetAccount.Balance -= transfer.Value;

        _db.Transfers.Remove(transfer);
        await _db.SaveChangesAsync(ct);

        HttpContext.Response.StatusCode = 204;
    }
}




