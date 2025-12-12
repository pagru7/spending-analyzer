using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Banks.Contracts;

namespace SpendingAnalyzer.Endpoints.Banks;

public class GetAllBanksEndpoint : EndpointWithoutRequest<List<BankResponse>>
{
    private readonly SpendingAnalyzerDbContext _db;
    private readonly ILogger<GetAllBanksEndpoint> _logger;
    

    public GetAllBanksEndpoint(SpendingAnalyzerDbContext db, ILogger<GetAllBanksEndpoint> logger)
    {
        _db = db;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/api/banks");
        AllowAnonymous();
        Description(q => q.WithTags("Banks").Produces<List<BankResponse>>(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Fetching banks started.");
            var banks = await _db.Banks
               .Include(b => b.BankAccounts)
               .AsNoTracking()
               .ToListAsync(ct);

            _logger.LogInformation("Fetched {BankCount} banks from database.", banks.Count);

            var response = banks.Select(bank => new BankResponse
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
            }).ToList();

            Response = response;

            _logger.LogInformation("Returning {BankCount} banks to caller.", response.Count);
        }
        catch (OperationCanceledException ex) when (ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "GetAllBanks request was canceled by the caller.");

            if (!HttpContext.Response.HasStarted)
                HttpContext.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
        }
    }
}




