using Csv;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Data;
using SpendingAnalyzer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpendingAnalyzer.Endpoints.Transactions
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
            if (req.Transactions is null)
                Response = new ImportTransactionsResponse(0);

            using var stream = req.Transactions?.OpenReadStream();
            // Read all bytes and decode text with UTF-8; if replacement chars appear, fallback to Windows-1250.
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            var bytes = ms.ToArray();

            string textUtf8 = Encoding.UTF8.GetString(bytes);
            string text;
            if (textUtf8.Contains('\uFFFD')) // replacement char present => likely wrong encoding
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                // Common for Polish bank CSV exports
                var cp1250 = Encoding.GetEncoding(1250); // "windows-1250"
                text = cp1250.GetString(bytes);
            }
            else
            {
                text = textUtf8;
            }

            

            var content = Csv.CsvReader.ReadFromText(text, new Csv.CsvOptions
            {
                Separator = ',',
                HeaderMode = Csv.HeaderMode.HeaderPresent,
                
            }).ToList();

            var bankAccountIdQuery = _db.BankAccounts
                .Include(ba => ba.Bank)
                .Where(b => b.Bank.Name.ToLower().Contains("inteligo") && b.Name.ToLower().Contains("osobiste"));

            _logger.LogInformation(bankAccountIdQuery.ToQueryString());
            var bankAccountId = bankAccountIdQuery.First().Id;

            var newTransactions = ProcessContent(content, bankAccountId, ct);
            var newTransactionIds = newTransactions?.Select(nt => nt.ExternalId);

            var notExistingExternalIdsQuery = _db.Transactions
                .Select(t => t.ExternalId)
                .Where(t => newTransactionIds.Contains(t));

            _logger.LogInformation(notExistingExternalIdsQuery.ToQueryString());

            var existingExternalIds = notExistingExternalIdsQuery.ToHashSet();
            var transactionsToAdd = newTransactions
                .Where(nt => !existingExternalIds.Contains(nt.ExternalId));

            await _db.Transactions.AddRangeAsync(transactionsToAdd);
            var addedTransactions = await _db.SaveChangesAsync();


            await Task.CompletedTask; // Placeholder for async operation.
            Response = new ImportTransactionsResponse(addedTransactions);
            
        }

        private IEnumerable<Transaction>? ProcessContent(IEnumerable<ICsvLine> content, Guid bankAccountId, CancellationToken ct)
            => content is null || !content.Any()
                ? null
                : content.Select(line => InitializeTransaction(line, bankAccountId));

        private Transaction InitializeTransaction(ICsvLine line, Guid bankAccountId)
        {
            var t = new Transaction
            {
                AccountId = bankAccountId,
                ExternalId = int.Parse(line[0]),
                IssueDate = DateTime.Parse(line[2],System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal).ToUniversalTime(),
                Type = line[3],
                //Amount = decimal.Parse(line[4]),
                Currency = Enum.Parse<Currency>(line[5]),
                //Balance = decimal.Parse(line[6]),
                IssuerBankAccountNumber = line[7],
                IssuerName = line[8],
                Description = line[9],
                Description2 = line[10],
            };


            if(double.TryParse(line[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var amount))
            {
                t.Amount = (decimal)amount;
            }
            else
            {
                _logger.LogError("Failed to parse Amount: {Amount}", line[4]);
            }

            if(double.TryParse(line[6], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var balance))
            {
                t.Balance = (decimal)balance;
            }
            else
            {
                _logger.LogError("Failed to parse Balance: {Balance}", line[6]);
            }

            return t;
        }
    }

    internal record ImportTransactionsResponse(int AddedTransactions);

    internal record ImportTransactionsRequest
    {
        public IFormFile Transactions { get; set; } = null!;
    }
}
