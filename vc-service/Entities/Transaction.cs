namespace SpendingAnalyzer.Entities;

public class Transaction : Entity
{
    public DateOnly TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Balance { get; set; }

    public int? ImportedTransactionId { get; set; }
    public ImportedTransaction? ImportedTransaction { get; set; }

    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
}
