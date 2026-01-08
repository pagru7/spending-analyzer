namespace SpendingAnalyzer.Entities;

public class Bank : Entity
{
    public string Name { get; set; } = string.Empty;
    public bool IsInactive { get; set; }

    // Navigation property
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}