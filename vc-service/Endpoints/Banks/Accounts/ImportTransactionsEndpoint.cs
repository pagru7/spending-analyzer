using Csv;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Services;

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
                ICsvLine[] content = await processor.GetContent(req.Transactions, ct);

                var parser = new InteligoTransactionImportDataParser();
                var parsedTransactions = content
                    .Select((line, index) => new
                    {
                        LineNumber = index + 1,
                        Result = parser.InitializeTransaction(line, bankAccount.Id)
                    })
                    .ToList();

                var failedParse = parsedTransactions.FirstOrDefault(x => !x.Result.IsSuccess);
                if (failedParse is not null)
                {
                    _logger.LogWarning(
                        "Transaction import parse failed at CSV line {LineNumber}. Error: {Error}",
                        failedParse.LineNumber,
                        failedParse.Result.Error);

                    ThrowError($"CSV parsing failed at line {failedParse.LineNumber}: {failedParse.Result.Error}");
                    return;
                }

                var newTransactions = parsedTransactions
                    .Select(x => x.Result.Value!)
                    .ToList();

                if (!newTransactions.Any())
                {
                    Response = new ImportTransactionsResponse(0);
                    return;
                }

                var newTransactionIds = newTransactions
                    .Where(nt => nt.ExternalIdParsed.HasValue)
                    .Select(nt => nt.ExternalIdParsed!.Value)
                    .Distinct()
                    .ToList();

                var existingExternalIds = await _db.ImportedTransactions
                    .Where(t => t.AccountId == bankAccount.Id && t.ExternalIdParsed.HasValue && newTransactionIds.Contains(t.ExternalIdParsed.Value))
                    .Select(t => t.ExternalIdParsed!.Value)
                    .ToHashSetAsync(ct);

                var transactionsToAdd = new List<SpendingAnalyzer.Entities.ImportedTransaction>();
                var batchExternalIds = new HashSet<int>();

                foreach (var transaction in newTransactions)
                {
                    if (!transaction.ExternalIdParsed.HasValue)
                    {
                        transactionsToAdd.Add(transaction);
                        continue;
                    }

                    var parsedId = transaction.ExternalIdParsed.Value;
                    if (existingExternalIds.Contains(parsedId))
                        continue;

                    if (!batchExternalIds.Add(parsedId))
                        continue;

                    transactionsToAdd.Add(transaction);
                }

                if (transactionsToAdd.Count > 0)
                {
                    await _db.ImportedTransactions.AddRangeAsync(transactionsToAdd, ct);
                    var addedTransactions = await _db.SaveChangesAsync(ct);

                    Response = new ImportTransactionsResponse(addedTransactions);
                }
                else
                {
                    Response = new ImportTransactionsResponse(0);
                }
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

