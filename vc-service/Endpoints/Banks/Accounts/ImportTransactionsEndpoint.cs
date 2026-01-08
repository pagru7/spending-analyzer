using Csv;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Common;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Entities;
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
            Post("/api/transactions/import");
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

                using var stream = req.Transactions.OpenReadStream();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms, ct);
                var bytes = ms.ToArray();

                string text;
                string textUtf8 = Encoding.UTF8.GetString(bytes);
                if (textUtf8.Contains('\uFFFD'))
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    var cp1250 = Encoding.GetEncoding(1250);
                    text = cp1250.GetString(bytes);
                }
                else
                {
                    text = textUtf8;
                }

                var content = CsvReader.ReadFromText(text, new CsvOptions
                {
                    Separator = ',',
                    HeaderMode = HeaderMode.HeaderPresent,
                }).ToArray();

                var bankAccountIdQuery = _db.Accounts
                    .Include(ba => ba.Bank)
                    .Where(b => b.Bank.Name.ToLower().Contains("inteligo") && b.Name.ToLower().Contains("osobiste"));

                _logger.LogInformation(bankAccountIdQuery.ToQueryString());
                var bankAccount = await bankAccountIdQuery.FirstOrDefaultAsync(ct);
                if (bankAccount is null)
                {
                    _logger.LogWarning("Bank account for import not found.");
                    Response = new ImportTransactionsResponse(0);
                    return;
                }

                var newTransactions = ProcessContent(content, bankAccount.Id, ct)?.ToList();
                if (newTransactions is null || !newTransactions.Any())
                {
                    Response = new ImportTransactionsResponse(0);
                    return;
                }

                var newTransactionIds = newTransactions
                    .Select(nt => nt.ExternalId).ToList();

                var existingExternalIds = await _db.ImportedTransactions
                    .Where(t => newTransactionIds.Contains(t.ExternalId))
                    .Select(t => t.ExternalId)
                    .ToHashSetAsync(ct);

                var transactionsToAdd = newTransactions
                    .Where(nt => !existingExternalIds.Contains(nt.ExternalId));

                await _db.ImportedTransactions
                    .AddRangeAsync(transactionsToAdd, ct);
                var addedTransactions = await _db.SaveChangesAsync(ct);

                Response = new ImportTransactionsResponse(addedTransactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during transaction import.");
                ThrowError("An unexpected error occurred. Please try again later.");
            }
        }

        private IEnumerable<ImportedTransaction>? ProcessContent(IEnumerable<ICsvLine> content, int bankAccountId, CancellationToken ct)
            => content?.Select(line => InitializeTransaction(line, bankAccountId)).Where(t => t is not null)!;

        private ImportedTransaction? InitializeTransaction(ICsvLine line, int bankAccountId)
        {
            if (!int.TryParse(line[0], out var externalId))
            {
                _logger.LogError("Failed to parse ExternalId: {ExternalId}", line[0]);
                return null;
            }

            if (!DateTime.TryParse(line[2], CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var issueDate))
            {
                _logger.LogError("Failed to parse IssueDate: {IssueDate}", line[2]);
                return null;
            }

            var csvType = line[3].ToLower();
            if (!ImportTransactionTypeMapping.Mapping.TryGetValue(csvType, out var transactionType))
            {
                _logger.LogWarning("Could not map transaction type: {CsvType}", line[3]);
                return null;
            }

            if (!Enum.TryParse<Currency>(line[5], true, out var currency))
            {
                _logger.LogError("Failed to parse Currency: {Currency}", line[5]);
                return null;
            }

            if (!decimal.TryParse(line[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var amount))
            {
                _logger.LogError("Failed to parse Amount: {Amount}", line[4]);
                return null;
            }

            if (!decimal.TryParse(line[6], NumberStyles.Float, CultureInfo.InvariantCulture, out var balance))
            {
                _logger.LogError("Failed to parse Balance: {Balance}", line[6]);
                return null;
            }

            return new ImportedTransaction
            {
                AccountId = bankAccountId,
                ExternalId = externalId,
                IssueDate = issueDate.ToUniversalTime(),
                Type = transactionType,
                Amount = amount,
                Currency = currency,
                Balance = balance,
                IssuerBankAccountNumber = line[7],
                IssuerName = line[8],
                Description = line[9],
                Description2 = line[10],
            };
        }
    }

    internal record ImportTransactionsResponse(int AddedTransactions);

    internal record ImportTransactionsRequest
    {
        public IFormFile Transactions { get; set; } = null!;
    }
}