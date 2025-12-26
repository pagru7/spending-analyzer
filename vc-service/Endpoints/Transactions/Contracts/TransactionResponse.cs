namespace SpendingAnalyzer.Endpoints.Transactions.Contracts;

public record TransactionResponse : TransactionResponseBase
{
    public Guid Id { get; set; }
    public string AccountName { get; set; } = string.Empty;
}