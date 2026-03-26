using MultiTenantManagement.Core.Enums;

namespace MultiTenantManagement.Data.Models;

public class Order
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public Guid CustomerId { get; init; }
    public required string DeliveryAddress { get; set; }
    public string Status { get; set; } = OrderStatus.PendingApproval.ToString();
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public Tenant? Tenant { get; init; }
    public ICollection<OrderItems> Items { get; init; } = new List<OrderItems>();
    public ICollection<OrderStatusHistory> StatusHistory { get; init; } = new List<OrderStatusHistory>();
    public Payment? Payment { get; set; }
}
