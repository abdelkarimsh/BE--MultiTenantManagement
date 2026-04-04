using Microsoft.EntityFrameworkCore;
using MultiTenantManagement.Core.Enums;
using MultiTenantManagement.Core.Extensions;
using MultiTenantManagement.Data;
using MultiTenantManagement.Data.Models;
using MultiTenantManagement.Infrastructure.Features.Order.Dtos;
using MultiTenantManagement.Infrastructure.Features.Order.Exceptions;
using MultiTenantManagement.Infrastructure.Features.Order.Helpers;
using MultiTenantManagement.Infrastructure.Features.Tenant;
using OrderEntity = MultiTenantManagement.Data.Models.Order;

namespace MultiTenantManagement.Infrastructure.Features.Order;

public class OrderService : IOrderService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantService _tenantService;

    public OrderService(AppDbContext dbContext, ITenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<OrderEntity> CreateOrderAsync(
     CreateOrderRequest request,
     Guid tenantId,
     Guid userId,
     CancellationToken cancellationToken = default)
    {
        EnsureTenantAccess(tenantId);

        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (request.CustomerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required.", nameof(request.CustomerId));

        if (string.IsNullOrWhiteSpace(request.DeliveryAddress))
            throw new ArgumentException("DeliveryAddress is required.", nameof(request.DeliveryAddress));

        if (request.Items == null || !request.Items.Any())
            throw new ArgumentException("Order must contain at least one item.", nameof(request.Items));

        var duplicateProducts = request.Items
            .GroupBy(x => x.ProductId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateProducts.Any())
            throw new ArgumentException("Duplicate products are not allowed in the order.");

        var order = new OrderEntity
        {
            TenantId = tenantId,
            CustomerId = request.CustomerId,
            DeliveryAddress = request.DeliveryAddress.Trim(),
            Status = OrderStatus.PendingApproval.ToString()
        };

        foreach (var item in request.Items)
        {
            if (item.ProductId == Guid.Empty)
                throw new ArgumentException("Each item must include ProductId.");

            if (item.Quantity <= 0)
                throw new ArgumentException("Each item must have Quantity greater than zero.");

            var product = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == item.ProductId && p.TenantId == tenantId, cancellationToken);

            if (product == null)
                throw new InvalidOperationException($"Product {item.ProductId} not found.");

            order.Items.Add(new OrderItems
            {
                TenantId = tenantId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
        }

        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return order;
    }

    public Task ApproveOrderAsync(Guid orderId, Guid tenantId, Guid userId,bool isTenantAdmin)
        => ChangeStatusAsync(orderId, tenantId, userId,isTenantAdmin, OrderStatus.Approved, "Approve", null);

    public Task RejectOrderAsync(Guid orderId, Guid tenantId, Guid userId, bool isTenantAdmin, string reason)
        => ChangeStatusAsync(orderId, tenantId, userId, isTenantAdmin, OrderStatus.Rejected, "Reject", reason);

    public Task CancelOrderAsync(Guid orderId, Guid tenantId, Guid userId, bool isTenantAdmin, string reason)
        => ChangeStatusAsync(orderId, tenantId, userId, isTenantAdmin, OrderStatus.Cancelled, "Cancel", reason);

    public async Task<OrderDto> GetOrderByIdAsync(Guid orderId, Guid tenantId,Guid currentUserId, bool isTenantAdmin, CancellationToken ct = default)
    {
        EnsureTenantAccess(tenantId);

        var query = _dbContext.Orders
            .AsNoTracking()
            .Where(x => x.Id == orderId && x.TenantId == tenantId);

        if (!isTenantAdmin)
        {
            query = query.Where(x => x.CustomerId == currentUserId);
        }

        var order = await query
            .Select(x => new OrderDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                CustomerId = x.CustomerId,
                DeliveryAddress = x.DeliveryAddress,
                Status = x.Status,
                TotalAmount = x.TotalAmount,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc,
                StatusHistory = x.StatusHistory
                    .OrderBy(h => h.ChangedAtUtc)
                    .Select(h => new OrderStatusHistoryDto
                    {
                        Id = h.Id,
                        FromStatus = h.FromStatus,
                        ToStatus = h.ToStatus,
                        ActionName = h.ActionName,
                        Comment = h.Comment,
                        ChangedBy = h.ChangedBy,
                        ChangedAtUtc = h.ChangedAtUtc
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        return order ?? throw new OrderNotFoundException(orderId, tenantId);
    }

    private async Task ChangeStatusAsync(Guid orderId, Guid tenantId, Guid userId,bool isTenantAdmin, OrderStatus nextStatus, string actionName, string? comment)
    {
        EnsureTenantAccess(tenantId);

        if ((nextStatus == OrderStatus.Rejected || nextStatus == OrderStatus.Cancelled) && string.IsNullOrWhiteSpace(comment))
            throw new ArgumentException("A reason is required.", nameof(comment));
        var query = _dbContext.Orders
          .Where(x => x.Id == orderId && x.TenantId == tenantId);

        if (!isTenantAdmin)
            query = query.Where(x => x.CustomerId == userId);

        var order = await query.FirstOrDefaultAsync();

        if (order is null)
            throw new OrderNotFoundException(orderId, tenantId);

        var currentStatus = order.Status;
        if (!OrderWorkflowRules.CanTransition(currentStatus.ToEnum<OrderStatus>(), nextStatus))
            throw new InvalidOrderTransitionException(orderId, currentStatus.ToEnum<OrderStatus>(), nextStatus);

        order.Status = nextStatus.ToString();
        order.UpdatedAtUtc = DateTime.UtcNow;

        _dbContext.OrderStatusHistories.Add(new OrderStatusHistory
        {
            TenantId = tenantId,
            OrderId = order.Id,
            FromStatus = currentStatus,
            ToStatus = nextStatus.ToString(),
            ActionName = actionName,
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            ChangedBy = userId,
            ChangedAtUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
    }

    private void EnsureTenantAccess(Guid tenantId)
    {
        var hasAccess = _tenantService.HasTenantAccessForCurrentUser(tenantId);
        if (!hasAccess)
            throw new UnauthorizedAccessException("User does not have access to this tenant.");
    }
}
