using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Banks.Contracts;

namespace SpendingAnalyzer.Endpoints.Banks;

public class UpdateBankEndpoint : Endpoint<UpdateBankRequest, BankResponse>
{
    private readonly SpendingAnalyzerDbContext _db;

    public UpdateBankEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Put("/api/banks/{id}");
        AllowAnonymous();
        Description(q => q.WithTags("Banks").Produces<BankResponse>(200).Produces(404));
    }

    public override async Task HandleAsync(UpdateBankRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var bank = await _db.Banks
            .Include(b => b.BankAccounts)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (bank is null)
        {
            HttpContext.Response.StatusCode = 404;
            return;
        }

        bank.Name = req.Name;
        await _db.SaveChangesAsync(ct);

        Response = new BankResponse
        {
            Id = bank.Id,
            Name = bank.Name,
            IsInactive = bank.IsInactive,
            BankAccounts = bank.BankAccounts.Select(a => new BankAccountResponse
            {
                Id = a.Id,
                Name = a.Name,
                CreationDate = a.CreationDate,
                Balance = a.Balance,
                IsInactive = a.IsInactive
            }).ToList()
        };
    }
}




