using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Endpoints.Transfers.Contracts;

namespace SpendingAnalyzer.Endpoints.Transfers;

public class GetAllTransfersEndpoint : EndpointWithoutRequest<List<TransferResponse>>
{
    private readonly SpendingAnalyzerDbContext _db;
    private readonly ILogger<GetAllTransfersEndpoint> _logger;

    public GetAllTransfersEndpoint(SpendingAnalyzerDbContext db, ILogger<GetAllTransfersEndpoint> logger)
    {
        _db = db;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/api/transfers");
        AllowAnonymous();
        Description(q => q.WithTags("Transfers").Produces<List<TransferResponse>>(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Fetching all transfers.");
            var transfers = await _db.Transfers
                .Include(t => t.SourceAccount)
                .Include(t => t.TargetAccount)
                .AsNoTracking()
                .ToListAsync(ct);

            _logger.LogInformation("Fetched {TransferCount} transfers from the database.", transfers.Count);

            var response = transfers.Select(t => new TransferResponse
            {
                Id = t.Id,
                Description = t.Description,
                SourceAccountId = t.SourceAccountId,
                SourceAccountName = t.SourceAccount.Name,
                TargetAccountId = t.TargetAccountId,
                TargetAccountName = t.TargetAccount.Name,
                Value = t.Value
            }).ToList();

            Response = response;
            _logger.LogInformation("Returning {TransferCount} transfers.", response.Count);
        }
        catch (OperationCanceledException ex) when (ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Request was canceled by the caller.");
            if (!HttpContext.Response.HasStarted)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
        }
    }
}




