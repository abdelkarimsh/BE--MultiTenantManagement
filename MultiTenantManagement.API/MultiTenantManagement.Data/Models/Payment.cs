namespace MultiTenantManagement.Data.Models;

public class Payment
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public Guid OrderId { get; init; }
    public required string Method { get; set; }
    public required string Status { get; set; }
    public required string TransactionReference { get; set; }
    public DateTime PaidAt { get; init; } = DateTime.UtcNow;

    public Order? Order { get; init; }
}
