using MultiTenantManagement.Core.Enums;

namespace MultiTenantManagement.Data.Models;

public class OrderStatusHistory
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public Guid OrderId { get; init; }
    public string FromStatus { get; init; }
    public string ToStatus { get; init; }
    public required string ActionName { get; init; }
    public string? Comment { get; init; }
    public Guid ChangedBy { get; init; }
    public DateTime ChangedAtUtc { get; init; } = DateTime.UtcNow;

    public Order? Order { get; init; }
}
