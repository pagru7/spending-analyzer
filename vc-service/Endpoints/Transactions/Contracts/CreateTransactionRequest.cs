namespace SpendingAnalyzer.Endpoints.Transactions.Contracts;

public record CreateTransactionRequest : TransactionResponseBase
{
    public short Type { get; set; }
}
