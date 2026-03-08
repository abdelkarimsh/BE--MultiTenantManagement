namespace MultiTenantManagement.Data.Models;

public class Product
{
    public Guid Id { get; init; } =  Guid.NewGuid();
    public Guid TenantId { get; init; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public int? StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }

    public Tenant? Tenant { get; init; }
    public ICollection<OrderItems> OrderItems { get; init; } = new List<OrderItems>();
}
