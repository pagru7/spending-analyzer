namespace SpendingAnalyzer.Endpoints.BankAccounts.Contracts;

public class CreateBankAccountRequest
{
    public Guid BankId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

public class UpdateBankAccountRequest
{
    public string Name { get; set; } = string.Empty;
}

public class BankAccountDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public decimal Balance { get; set; }
    public bool IsInactive { get; set; }
    public Guid BankId { get; set; }
    public string BankName { get; set; } = string.Empty;
}




