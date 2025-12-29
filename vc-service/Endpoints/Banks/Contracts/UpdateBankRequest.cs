namespace SpendingAnalyzer.Endpoints.Banks.Contracts;

public record UpdateBankRequest
{
    public string Name { get; set; } = string.Empty;
}
