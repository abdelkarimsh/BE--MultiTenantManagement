namespace MultiTenantManagement.Data.Models;

public class Order
{
    public Guid Id { get; init; } =  Guid.NewGuid();
    public Guid TenantId { get; init; }
    public Guid CustomerId { get; init; }
    public required string DeliveryAddress { get; set; }
    public required string Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public Tenant? Tenant { get; init; }
    public ICollection<OrderItems> Items { get; init; } = new List<OrderItems>();
    public Payment? Payment { get; set; }
}
