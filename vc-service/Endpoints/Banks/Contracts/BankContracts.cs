namespace SpendingAnalyzer.Endpoints.Banks.Contracts;

public class CreateBankRequest
{
    public string Name { get; set; } = string.Empty;
    public List<BankAccountDto>? BankAccounts { get; set; }
}

public class BankAccountDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

public class UpdateBankRequest
{
    public string Name { get; set; } = string.Empty;
}

public class BankResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsInactive { get; set; }
    public List<BankAccountResponse> BankAccounts { get; set; } = new();
}

public class BankAccountResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public decimal Balance { get; set; }
    public bool IsInactive { get; set; }
}




