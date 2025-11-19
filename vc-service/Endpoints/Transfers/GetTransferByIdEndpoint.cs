using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Transfers.Contracts;

namespace SpendingAnalyzer.Endpoints.Transfers;

public class GetTransferByIdEndpoint : EndpointWithoutRequest<TransferResponse>
{
    private readonly SpendingAnalyzerDbContext _db;

    public GetTransferByIdEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/transfers/{id}");
        AllowAnonymous();
        Description(q => q.WithTags("Transfers").Produces<TransferResponse>(200).Produces(404));
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

        Response = new TransferResponse
        {
            Id = transfer.Id,
            Description = transfer.Description,
            SourceAccountId = transfer.SourceAccountId,
            SourceAccountName = transfer.SourceAccount.Name,
            TargetAccountId = transfer.TargetAccountId,
            TargetAccountName = transfer.TargetAccount.Name,
            Value = transfer.Value
        };
    }
}




