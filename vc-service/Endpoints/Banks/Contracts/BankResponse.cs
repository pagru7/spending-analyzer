namespace SpendingAnalyzer.Endpoints.Banks.Contracts;

public record BankResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsInactive { get; set; }
    public IEnumerable<BankAccountResponse>? Accounts { get; set; }
}
