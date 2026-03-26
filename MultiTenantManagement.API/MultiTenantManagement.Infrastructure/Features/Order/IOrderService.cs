using MultiTenantManagement.Infrastructure.Features.Order.Dtos;
using OrderEntity = MultiTenantManagement.Data.Models.Order;

namespace MultiTenantManagement.Infrastructure.Features.Order;

public interface IOrderService
{
    Task<OrderEntity> CreateOrderAsync(
        CreateOrderRequest request,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default);
    Task ApproveOrderAsync(Guid orderId, Guid tenantId, Guid userId);
    Task RejectOrderAsync(Guid orderId, Guid tenantId, Guid userId, string reason);
    Task CancelOrderAsync(Guid orderId, Guid tenantId, Guid userId, string reason);
    Task<OrderDto> GetOrderByIdAsync(Guid orderId, Guid tenantId);
}
