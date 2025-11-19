namespace SpendingAnalyzer.Entities;

public class Transfer
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Value { get; set; }
    
    public Guid SourceAccountId { get; set; }
    public BankAccount SourceAccount { get; set; } = null!;
    
    public Guid TargetAccountId { get; set; }
    public BankAccount TargetAccount { get; set; } = null!;
}
