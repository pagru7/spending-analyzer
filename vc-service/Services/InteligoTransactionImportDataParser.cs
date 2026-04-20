using Csv;
using SpendingAnalyzer.Common;
using SpendingAnalyzer.Entities;

namespace SpendingAnalyzer.Services
{
    internal class InteligoTransactionImportDataParser
    {
        public enum ImportedTransactionDataAdnotation
        {
            ExternalId = 0,
            IssueDate = 2,
            TransactionType = 3,
            Amount = 4,
            Currency = 5,
            Balance = 6,
            BankAccount = 7,
            Name = 8,
            Description = 9,
            Description2 = 10,
        }

        internal readonly record struct ParseResult<T>(bool IsSuccess, T? Value, string? Error)
        {
            public static ParseResult<T> Success(T value) => new(true, value, null);
            public static ParseResult<T> Failure(string error) => new(false, default, error);
        }

        public ParseResult<ImportedTransaction> InitializeTransaction(
            ICsvLine line,
            int bankAccountId)
        {
            line.TryGetString(ImportedTransactionDataAdnotation.ExternalId, out var externalId);
            line.TryGetString(ImportedTransactionDataAdnotation.IssueDate, out var issueDate);
            line.TryGetString(ImportedTransactionDataAdnotation.TransactionType, out var transactionType);
            line.TryGetString(ImportedTransactionDataAdnotation.Amount, out var amount);
            line.TryGetString(ImportedTransactionDataAdnotation.Currency, out var currency);
            line.TryGetString(ImportedTransactionDataAdnotation.Balance, out var balance);
            line.TryGetString(ImportedTransactionDataAdnotation.BankAccount, out var bankAccountNumber);
            line.TryGetString(ImportedTransactionDataAdnotation.Name, out var issuerName);
            line.TryGetString(ImportedTransactionDataAdnotation.Description, out var description);
            line.TryGetString(ImportedTransactionDataAdnotation.Description2, out var description2);

            var rawExternalId = externalId?.Trim() ?? string.Empty;
            int? externalIdParsed = null;

            if (!string.IsNullOrWhiteSpace(rawExternalId))
            {
                if (!int.TryParse(rawExternalId, out var parsedExternalId))
                {
                    throw new FormatException($"Cannot parse ExternalId '{rawExternalId}' to integer.");
                }

                externalIdParsed = parsedExternalId;
            }

            var transaction = new ImportedTransaction
            {
                AccountId = bankAccountId,
                ExternalId = rawExternalId,
                ExternalIdParsed = externalIdParsed,
                IssueDate = issueDate ?? string.Empty,
                Type = transactionType ?? string.Empty,
                Amount = amount ?? string.Empty,
                Currency = currency ?? string.Empty,
                Balance = balance ?? string.Empty,
                IssuerBankAccountNumber = bankAccountNumber ?? string.Empty,
                IssuerName = issuerName ?? string.Empty,
                Description = description ?? string.Empty,
                Description2 = description2 ?? string.Empty,
            };

            return ParseResult<ImportedTransaction>.Success(transaction);
        }
    }
}
