using SpendingAnalyzer.Endpoints.Banks.Contracts;
using SpendingAnalyzer.Entities;

namespace SpendingAnalyzer.Endpoints.Banks;

public record CreateBankRequest
{
    public string Name { get; set; } = string.Empty;
    public List<BankAccountDto>? Accounts { get; set; }

    internal Bank ToBank()
    => new Bank
    {
        Name = this.Name,
        IsInactive = false,
        CreatedAt = DateTime.UtcNow,
        Accounts = Accounts?.Select(a => new Account
        {
            Name = a.Name,
            IsInactive = false,
            CreatedAt = DateTime.UtcNow,
            Transactions = new List<Transaction>
            {
                new Transaction
                {
                    Amount = a.Balance,
                    Balance = a.Balance,
                    CreatedAt = DateTime.UtcNow,
                    Description = $"Initial balance for account {a.Name}",
                    TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    Recipient = "Initial Balance"
                }
            }
        })?.ToArray() ?? Array.Empty<Account>()
    };
}