namespace SpendingAnalyzer.Entities;

public class Bank
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsInactive { get; set; }
    
    // Navigation property
    public ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
}
