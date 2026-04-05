using MultiTenantManagement.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace MultiTenantManagement.Infrastructure.Features.Order.Dtos;

public class CreateOrderRequest
{
    [Required]
    public Guid CustomerId { get; set; }
    [Required]
    public string DeliveryAddress { get; set; } = string.Empty;
    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; }
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public class CreateOrderItemRequest
{
    [Required]
    public Guid ProductId { get; set; }
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }
}

public class RejectOrderRequest
{
    [Required]
    [MinLength(3)]
    public string Reason { get; set; } = string.Empty;
}

public class CancelOrderRequest
{
    [Required]
    [MinLength(3)]
    public string Reason { get; set; } = string.Empty;
}

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new();
}

public class OrderListItemDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
public class OrderStatusHistoryDto
{
    public Guid Id { get; set; }
    public string FromStatus { get; set; }
    public string ToStatus { get; set; }
    public string ActionName { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAtUtc { get; set; }
}

