namespace SpendingAnalyzer.Entities;

public class Transaction
{
    /// <summary>
    /// Gets or sets the unique identifier of the transaction.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the date when the issue was created or recorded.
    /// </summary>
    public DateTime IssueDate { get; set; }

    /// <summary>
    /// Gets or sets the external identifier associated with the entity.
    /// </summary>
    public int ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the type identifier associated with the current instance.
    /// </summary>
    public short? Type { get; set; } 

    /// <summary>
    /// Gets or sets the monetary amount associated with the transaction.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency associated with the transaction or value.
    /// </summary>
    public Currency Currency { get; set; }

    /// <summary>
    /// Balance after the transaction
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Gets or sets the bank account number of the issuer associated with the transaction.
    /// </summary>
    public string IssuerBankAccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the issuer associated with the transaction.
    /// </summary>
    public string IssuerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary description of the transaction.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an additional description or details for the transaction.
    /// </summary>
    public string Description2 { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transaction recipient or counterparty name.
    /// </summary>
    public string Recipient { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the associated bank account.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Gets or sets the bank account associated with the transaction.
    /// </summary>
    public BankAccount Account { get; set; } = null!;
}

public enum Currency
{
    PLN,
    EUR,
    USD,
    GBP
}
