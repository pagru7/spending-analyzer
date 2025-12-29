using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Banks.Contracts;

namespace SpendingAnalyzer.Endpoints.Banks;

public class GetBankByIdEndpoint : EndpointWithoutRequest<BankResponse>
{
    private readonly SpendingAnalyzerDbContext _db;

    public GetBankByIdEndpoint(SpendingAnalyzerDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/banks/{id}");
        AllowAnonymous();
        Description(q => q.WithTags("Banks").Produces<BankResponse>(200).Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");

        var bank = await _db.Banks
            .Include(b => b.Accounts)
            .Include(t => t.Accounts)
               .Where(t => t.Accounts.Where(a => a.Transactions.FirstOrDefault() != null).Any())
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (bank is null)
        {
            HttpContext.Response.StatusCode = 404;
            return;
        }

        Response = new BankResponse
        {
            Id = bank.Id,
            Name = bank.Name,
            IsInactive = bank.IsInactive,
            BankAccounts = bank.Accounts.Select(a => new BankAccountResponse
            {
                Id = a.Id,
                Name = a.Name,
                CreationDate = a.CreatedAt,
                Balance = a.Transactions.FirstOrDefault()?.Balance ?? 0,
                IsInactive = a.IsInactive
            }).ToList()
        };
    }
}





