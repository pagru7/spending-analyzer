using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Transfers.Contracts;

namespace SpendingAnalyzer.Endpoints.Transfers;

public class GetAllTransfersEndpoint : EndpointWithoutRequest<List<TransferResponse>>
{
    private readonly SpendingAnalyzerDbContext _db;

    public GetAllTransfersEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/transfers");
        AllowAnonymous();
        Description(q => q.WithTags("Transfers").Produces<List<TransferResponse>>(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var transfers = await _db.Transfers
            .Include(t => t.SourceAccount)
            .Include(t => t.TargetAccount)
            .ToListAsync(ct);

        Response = transfers.Select(t => new TransferResponse
        {
            Id = t.Id,
            Description = t.Description,
            SourceAccountId = t.SourceAccountId,
            SourceAccountName = t.SourceAccount.Name,
            TargetAccountId = t.TargetAccountId,
            TargetAccountName = t.TargetAccount.Name,
            Value = t.Value
        }).ToList();
    }
}




