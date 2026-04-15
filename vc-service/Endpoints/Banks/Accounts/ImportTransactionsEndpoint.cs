using Csv;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Common;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Entities;
using SpendingAnalyzer.Services;
using System.Globalization;
using System.Text;

namespace SpendingAnalyzer.Endpoints.Banks.Accounts
{
    internal class ImportTransactionsEndpoint : Endpoint<ImportTransactionsRequest, ImportTransactionsResponse>
    {
        private readonly SpendingAnalyzerDbContext _db;
        private readonly ILogger<ImportTransactionsEndpoint> _logger;

        public ImportTransactionsEndpoint(
            SpendingAnalyzerDbContext db,
            ILogger<ImportTransactionsEndpoint> logger)
        {
            _db = db;
            _logger = logger;
        }

        public override void Configure()
        {
            Post(ApiRoutes.BankAccountByIdTransactionImport);
            AllowAnonymous();
            AllowFileUploads();
            Description(q => q.WithTags("Transactions").Produces<ImportTransactionsRequest>(200));
        }

        public override async Task HandleAsync(ImportTransactionsRequest req, CancellationToken ct)
        {
            try
            {
                if (req.Transactions is null)
                {
                    Response = new ImportTransactionsResponse(0);
                    return;
                }

                var bankId = Route<int>("bankId");
                var accountId = Route<int>("accountId");

                var bankAccount = await _db.Accounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == accountId && a.BankId == bankId, ct);

                if (bankAccount is null)
                {
                    _logger.LogWarning("Bank account not found for import. BankId: {BankId}, AccountId: {AccountId}", bankId, accountId);
                    HttpContext.Response.StatusCode = 404;
                    return;
                }

                var processor = new TransactionImportProcessor();
                ICsvLine[] content = await processor.GetContent(
                    req.Transactions,
                    ct);



                //var newTransactions = ProcessContent(content, bankAccount.Id)?.ToList();
                InteligoTransactionImportDataParser parser = new InteligoTransactionImportDataParser();
                var newTransactions = content.Select(cline => parser.InitializeTransaction(cline, bankAccount.Id));
                if (newTransactions is null || !newTransactions.Any())
                {
                    Response = new ImportTransactionsResponse(0);
                    return;
                }

                var newTransactionIds = newTransactions
                    .Select(nt => nt.ExternalId)
                    .ToList();

                var existingExternalIds = await _db.ImportedTransactions
                    .Where(t => newTransactionIds.Contains(t.ExternalId))
                    .Select(t => t.ExternalId)
                    .ToHashSetAsync(ct);

                var transactionsToAdd = newTransactions
                    .Where(nt => !existingExternalIds.Contains(nt.ExternalId));

                await _db.ImportedTransactions.AddRangeAsync(transactionsToAdd, ct);
                var addedTransactions = await _db.SaveChangesAsync(ct);

                Response = new ImportTransactionsResponse(addedTransactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during transaction import.");
                ThrowError("An unexpected error occurred. Please try again later.");
            }
        }        
    }

    internal record ImportTransactionsResponse(int AddedTransactions);

    internal record ImportTransactionsRequest
    {
        public IFormFile Transactions { get; set; } = null!;
    }
}