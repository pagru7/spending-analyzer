using Csv;
using SpendingAnalyzer.Common;
using SpendingAnalyzer.Entities;
using System.Globalization;

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

        public ImportedTransaction? InitializeTransaction(
            ICsvLine line,
            int bankAccountId)
        {
            if (!line.TryGetInt(ImportedTransactionDataAdnotation.ExternalId, out var externalId))
                return null;

            if (!line.TryGetDate(ImportedTransactionDataAdnotation.IssueDate, out var issueDate))
                return null;

            if (!line.TryGetTransactionType(ImportedTransactionDataAdnotation.TransactionType, out var transactionType))
                return null;

            if (!line.TryGetDecimal(ImportedTransactionDataAdnotation.Amount, out var amount))
                return null;

            if (!line.TryGetCurrency(ImportedTransactionDataAdnotation.Currency, out var currency))
                return null;

            if (!line.TryGetDecimal(ImportedTransactionDataAdnotation.Balance, out var balance))
                return null;

            line.TryGetString(ImportedTransactionDataAdnotation.BankAccount, out var bankAccountNumber);
            line.TryGetString(ImportedTransactionDataAdnotation.Name, out var issuerName);
            line.TryGetString(ImportedTransactionDataAdnotation.Description, out var description);
            line.TryGetString(ImportedTransactionDataAdnotation.Description2, out var description2);

            return new ImportedTransaction
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
        }
    }
}
