using SQLite;

namespace Spending_Analyzer_Mobile.Models;

public class AppSettings
{
    [PrimaryKey]
    public int Id { get; set; } = 1;

    public string HostUrl { get; set; } = string.Empty;

    public int Port { get; set; } = 8080;

    public string AccountId { get; set; } = string.Empty;

    public string AccountName { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;
}
