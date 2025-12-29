namespace SpendingAnalyzer.Endpoints.Banks.Contracts;

public record BankAccountResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public decimal Balance { get; set; }
    public bool IsInactive { get; set; }
}