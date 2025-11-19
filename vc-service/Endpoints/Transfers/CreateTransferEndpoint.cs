using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Transfers.Contracts;
using SpendingAnalyzer.Entities;

namespace SpendingAnalyzer.Endpoints.Transfers;

public class CreateTransferEndpoint : Endpoint<CreateTransferRequest, TransferResponse>
{
    private readonly SpendingAnalyzerDbContext _db;

    public CreateTransferEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Post("/api/transfers");
        AllowAnonymous();
        Description(q => q.WithTags("Transfers").Produces<TransferResponse>(201));
    }

    public override async Task HandleAsync(CreateTransferRequest req, CancellationToken ct)
    {
        var sourceAccount = await _db.BankAccounts.FirstOrDefaultAsync(a => a.Id == req.SourceAccountId, ct);
        var targetAccount = await _db.BankAccounts.FirstOrDefaultAsync(a => a.Id == req.TargetAccountId, ct);

        if (sourceAccount is null)
        {
            AddError("SourceAccountId", "Source account not found");
            ThrowIfAnyErrors();
            return;
        }

        if (targetAccount is null)
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

        var transfer = new Transfer
        {
            Id = Guid.NewGuid(),
            Description = req.Description,
            SourceAccountId = req.SourceAccountId,
            TargetAccountId = req.TargetAccountId,
            Value = req.Value
        };

        // Update account balances
        sourceAccount.Balance -= req.Value;
        targetAccount.Balance += req.Value;

        _db.Transfers.Add(transfer);
        await _db.SaveChangesAsync(ct);

        Response = new TransferResponse
        {
            Id = transfer.Id,
            Description = transfer.Description,
            SourceAccountId = transfer.SourceAccountId,
            SourceAccountName = sourceAccount.Name,
            TargetAccountId = transfer.TargetAccountId,
            TargetAccountName = targetAccount.Name,
            Value = transfer.Value
        };
    }
}




