namespace SpendingAnalyzer.Endpoints.Transactions.Contracts;

public record TransactionResponse : TransactionResponseBase
{
    public int Id { get; set; }
    public string AccountName { get; set; } = string.Empty;
}