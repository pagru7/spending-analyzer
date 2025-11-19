namespace SpendingAnalyzer.Endpoints.Transfers.Contracts;

public class CreateTransferRequest
{
    public string Description { get; set; } = string.Empty;
    public Guid SourceAccountId { get; set; }
    public Guid TargetAccountId { get; set; }
    public decimal Value { get; set; }
}

public class UpdateTransferRequest
{
    public string Description { get; set; } = string.Empty;
    public Guid SourceAccountId { get; set; }
    public Guid TargetAccountId { get; set; }
    public decimal Value { get; set; }
}

public class TransferResponse
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid SourceAccountId { get; set; }
    public string SourceAccountName { get; set; } = string.Empty;
    public Guid TargetAccountId { get; set; }
    public string TargetAccountName { get; set; } = string.Empty;
    public decimal Value { get; set; }
}




