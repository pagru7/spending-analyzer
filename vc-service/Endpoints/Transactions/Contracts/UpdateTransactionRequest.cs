namespace SpendingAnalyzer.Endpoints.Transactions.Contracts;

public record UpdateTransactionRequest : TransactionResponseBase
{
    public bool IsIncome { get; set; }
}