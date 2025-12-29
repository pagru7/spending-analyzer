namespace SpendingAnalyzer.Entities;

public class Bank : Entity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsInactive { get; set; }
    
    // Navigation property
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}
