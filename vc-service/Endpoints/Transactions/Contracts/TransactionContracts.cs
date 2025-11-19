namespace SpendingAnalyzer.Endpoints.Transactions.Contracts;

public class CreateTransactionRequest
{
    public string Description { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class UpdateTransactionRequest
{
    public string Description { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class TransactionResponse
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}




