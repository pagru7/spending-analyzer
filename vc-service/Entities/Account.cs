namespace SpendingAnalyzer.Entities;

public class Account : Entity
{
    public string Name { get; set; } = string.Empty;

    public bool IsInactive { get; set; }

    public int BankId { get; set; }

    public Bank Bank { get; set; } = null!;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public IEnumerable<ImportedTransaction> ImportedTransactions { get; set; } = new List<ImportedTransaction>();
}