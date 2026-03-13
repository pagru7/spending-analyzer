using SQLite;

namespace Spending_Analyzer_Mobile.Models;

public class Transaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public string Recipient { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; } = DateTime.Now;

    public string TransactionType { get; set; } = TransactionTypes.Spending;

    public decimal Balance { get; set; }

    public bool IsSynchronized { get; set; }

    public DateTime? LastSyncDate { get; set; }

    public string? ServerId { get; set; }
}

public static class TransactionTypes
{
    public const string Spending = "Spending";
    public const string Income = "Income";
    public const string Transfer = "Transfer";
}
