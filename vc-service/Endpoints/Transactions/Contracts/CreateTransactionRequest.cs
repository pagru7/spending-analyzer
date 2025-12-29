using SpendingAnalyzer.Entities;

namespace SpendingAnalyzer.Endpoints.Transactions.Contracts;

public record CreateTransactionRequest : TransactionResponseBase
{
    public double? TransactionFee { get; set; }

    public bool IsIncome { get; set; }

    internal Transaction ToTransaction()
        => new Transaction
        {
            CreatedAt = DateTime.UtcNow,
            TransactionDate = this.TransactionDate,
            Amount = this.IsIncome ? this.Amount : -this.Amount,
            Recipient = this.Recipient,
            Description = this.Description,
            ImportedTransactionId = null,
            AccountId = this.AccountId,
        };
}