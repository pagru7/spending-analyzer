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
            if (!line.TryGetInt(ImportedTransactionDataAdnotation.ExternalId, out var externalId))
                return ParseResult<ImportedTransaction>.Failure("ExternalId cannot be parsed.");

            if (!line.TryGetDate(ImportedTransactionDataAdnotation.IssueDate, out var issueDate))
                return ParseResult<ImportedTransaction>.Failure("IssueDate cannot be parsed.");

            if (!line.TryGetTransactionType(ImportedTransactionDataAdnotation.TransactionType, out var transactionType))
                return ParseResult<ImportedTransaction>.Failure("TransactionType cannot be parsed.");

            if (!line.TryGetDecimal(ImportedTransactionDataAdnotation.Amount, out var amount))
                return ParseResult<ImportedTransaction>.Failure("Amount cannot be parsed.");

            if (!line.TryGetCurrency(ImportedTransactionDataAdnotation.Currency, out var currency))
                return ParseResult<ImportedTransaction>.Failure("Currency cannot be parsed.");

            if (!line.TryGetDecimal(ImportedTransactionDataAdnotation.Balance, out var balance))
                return ParseResult<ImportedTransaction>.Failure("Balance cannot be parsed.");

            line.TryGetString(ImportedTransactionDataAdnotation.BankAccount, out var bankAccountNumber);
            line.TryGetString(ImportedTransactionDataAdnotation.Name, out var issuerName);
            line.TryGetString(ImportedTransactionDataAdnotation.Description, out var description);
            line.TryGetString(ImportedTransactionDataAdnotation.Description2, out var description2);

            var transaction = new ImportedTransaction
            {
                AccountId = bankAccountId,
                ExternalId = externalId,
                IssueDate = issueDate.ToUniversalTime(),
                Type = transactionType,
                Amount = amount,
                Currency = currency,
                Balance = balance,
                IssuerBankAccountNumber = bankAccountNumber ?? string.Empty,
                IssuerName = issuerName ?? string.Empty,
                Description = description ?? string.Empty,
                Description2 = description2 ?? string.Empty,
            };

            return ParseResult<ImportedTransaction>.Success(transaction);
        }
    }
}
