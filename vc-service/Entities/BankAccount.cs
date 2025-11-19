namespace SpendingAnalyzer.Entities;

public class BankAccount
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public decimal Balance { get; set; }
    public bool IsInactive { get; set; }
    
    public Guid BankId { get; set; }
    public Bank Bank { get; set; } = null!;
    
    // Navigation properties
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Transfer> SourceTransfers { get; set; } = new List<Transfer>();
    public ICollection<Transfer> TargetTransfers { get; set; } = new List<Transfer>();
}
