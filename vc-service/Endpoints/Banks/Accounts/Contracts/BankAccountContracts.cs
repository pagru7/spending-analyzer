using SpendingAnalyzer.Entities;

namespace SpendingAnalyzer.Endpoints.Banks.Accounts.Contracts;

public record CreateBankAccountRequest
{
    public int BankId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }

    internal Account ToAccount()
        => new()
        {
            Name = Name,
            CreatedAt = DateTime.UtcNow,
            IsInactive = false,
            BankId = BankId
        };
}

public record UpdateBankAccountRequest
{
    public string Name { get; set; } = string.Empty;
}

public record BankAccountDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public decimal Balance { get; set; }
    public bool IsInactive { get; set; }
    public int BankId { get; set; }
    public string BankName { get; set; } = string.Empty;
}

public record DeleteAccountRequest
{
    public int BankId { get; set; }
    public int AccountId { get; set; }
}

public record UpdateAccountBalanceRequest
{
    public decimal NewBalance { get; set; }
    public string? Description { get; set; }
}