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

    public bool IsSynchronized { get; set; }

    public DateTime? LastSyncDate { get; set; }

    public string? ServerId { get; set; }
}
