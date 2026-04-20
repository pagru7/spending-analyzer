namespace SpendingAnalyzer.Endpoints.Transactions.Contracts;

public record ImportedTransactionResponse
{
    public int Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public int? ExternalIdParsed { get; set; }
    public string IssueDate { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Balance { get; set; } = string.Empty;
    public string IssuerBankAccountNumber { get; set; } = string.Empty;
    public string IssuerName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Description2 { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
}
