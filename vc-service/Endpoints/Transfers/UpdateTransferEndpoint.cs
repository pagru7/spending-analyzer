using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Transfers.Contracts;

namespace SpendingAnalyzer.Endpoints.Transfers;

public class UpdateTransferEndpoint : Endpoint<UpdateTransferRequest, TransferResponse>
{
    private readonly SpendingAnalyzerDbContext _db;

    public UpdateTransferEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Put("/api/transfers/{id}");
        AllowAnonymous();
        Description(q => q.WithTags("Transfers").Produces<TransferResponse>(200).Produces(404));
    }

    public override async Task HandleAsync(UpdateTransferRequest req, CancellationToken ct)
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

        var newSourceAccount = await _db.BankAccounts.FirstOrDefaultAsync(a => a.Id == req.SourceAccountId, ct);
        var newTargetAccount = await _db.BankAccounts.FirstOrDefaultAsync(a => a.Id == req.TargetAccountId, ct);

        if (newSourceAccount is null)
        {
            AddError("SourceAccountId", "Source account not found");
            ThrowIfAnyErrors();
            return;
        }

        if (newTargetAccount is null)
        {
            AddError("TargetAccountId", "Target account not found");
            ThrowIfAnyErrors();
            return;
        }

        if (req.SourceAccountId == req.TargetAccountId)
        {
            AddError("TargetAccountId", "Source and target accounts cannot be the same");
            ThrowIfAnyErrors();
            return;
        }

        // Revert old transfer balance changes
        transfer.SourceAccount.Balance += transfer.Value;
        transfer.TargetAccount.Balance -= transfer.Value;

        // Apply new transfer balance changes
        newSourceAccount.Balance -= req.Value;
        newTargetAccount.Balance += req.Value;

        // Update transfer
        transfer.Description = req.Description;
        transfer.SourceAccountId = req.SourceAccountId;
        transfer.TargetAccountId = req.TargetAccountId;
        transfer.Value = req.Value;

        await _db.SaveChangesAsync(ct);

        Response = new TransferResponse
        {
            Id = transfer.Id,
            Description = transfer.Description,
            SourceAccountId = transfer.SourceAccountId,
            SourceAccountName = newSourceAccount.Name,
            TargetAccountId = transfer.TargetAccountId,
            TargetAccountName = newTargetAccount.Name,
            Value = transfer.Value
        };
    }
}




