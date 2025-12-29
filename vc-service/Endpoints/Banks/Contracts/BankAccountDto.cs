namespace SpendingAnalyzer.Endpoints.Banks.Contracts;

public record BankAccountDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}
