namespace SpendingAnalyzer.Endpoints.Transactions.Contracts
{
    public record TransactionResponseBase
    {
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
    }
}