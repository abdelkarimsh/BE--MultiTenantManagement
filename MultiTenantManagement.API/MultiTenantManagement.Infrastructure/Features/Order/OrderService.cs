using Microsoft.EntityFrameworkCore;
using MultiTenantManagement.Core.Enums;
using MultiTenantManagement.Core.Extensions;
using MultiTenantManagement.Data;
using MultiTenantManagement.Data.Models;
using MultiTenantManagement.Infrastructure.Features.Order.Dtos;
using MultiTenantManagement.Infrastructure.Features.Order.Exceptions;
using MultiTenantManagement.Infrastructure.Features.Order.Helpers;
using MultiTenantManagement.Infrastructure.Helpers;
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
        await EnsureTenantAccessAsync(tenantId,cancellationToken);

        if (request is null)
            throw new ArgumentNullException(nameof(request));

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

        var productIds = request.Items.Select(i => i.ProductId).ToList();

        var products = await _dbContext.Products
            .Where(p => productIds.Contains(p.Id) && p.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var productDict = products.ToDictionary(p => p.Id);

        var order = new OrderEntity
        {
            TenantId = tenantId,
            CustomerId = userId,
            DeliveryAddress = request.DeliveryAddress.Trim(),
            Status = OrderStatus.PendingApproval.ToString(),
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var item in request.Items)
        {
            if (item.ProductId == Guid.Empty)
                throw new ArgumentException("Each item must include ProductId.");

            if (item.Quantity <= 0)
                throw new ArgumentException("Each item must have Quantity greater than zero.");

            if (!productDict.TryGetValue(item.ProductId, out var product))
                throw new InvalidOperationException($"Product {item.ProductId} not found.");

            if (product.IsDeleted || !product.IsActive)
                throw new InvalidOperationException($"Product {product.Id} is not available.");

            if (product.Version != item.ProductVersion)
                throw new InvalidOperationException(
                    $"Product {product.Name} was updated. Please refresh and try again.");

            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"Not enough stock for product {product.Name}.");

            product.StockQuantity -= item.Quantity;
            product.Version++;

            order.Items.Add(new OrderItems
            {
                TenantId = tenantId,
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });

        }

        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

        _dbContext.Orders.Add(order);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException(
                "Product stock was changed by another request. Please refresh and try again.");
        }

        return order;
    }

    public Task ApproveOrderAsync(Guid orderId, Guid tenantId, Guid userId, bool isTenantAdmin, int version)
        => ChangeStatusAsync(orderId, tenantId, userId, isTenantAdmin, OrderStatus.Approved, "Approve", null, version);

    public Task RejectOrderAsync(Guid orderId, Guid tenantId, Guid userId, bool isTenantAdmin, string reason, int version)
        => ChangeStatusAsync(orderId, tenantId, userId, isTenantAdmin, OrderStatus.Rejected, "Reject", reason, version);

    public Task CancelOrderAsync(Guid orderId, Guid tenantId, Guid userId, bool isTenantAdmin, string reason, int version)
        => ChangeStatusAsync(orderId, tenantId, userId, isTenantAdmin, OrderStatus.Cancelled, "Cancel", reason, version);
    public async Task<OrderDto> GetOrderByIdAsync(Guid orderId, Guid tenantId,Guid currentUserId, bool isTenantAdmin, CancellationToken ct = default)
    {
        await EnsureTenantAccessAsync(tenantId,ct);

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
                Version = x.Version,
                ListItems = x.Items.Select(i=> new OrderItemDto
                {
                    Id =i.Id,
                    OrderId =i.OrderId,
                    ProductName = i.Product != null ? i.Product.Name : string.Empty,
                    ProductId =i.ProductId,
                    Quantity =i.Quantity,
                    UnitPrice =i.UnitPrice
                }).ToList(),
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


    public async Task<PagedResult<OrderListItemDto>> GetOrdersAsync(
        Guid tenantId,
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool isAscending,
        string? search,
        string? status,
        Guid? customerId,
        CancellationToken ct = default)
    {
        await EnsureTenantAccessAsync(tenantId,ct);

        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        var query = _dbContext.Orders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (customerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == customerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim().ToLower();
            query = query.Where(x => x.Status.ToLower() == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            if (Guid.TryParse(search, out var searchOrderId))
            {
                query = query.Where(x => x.Id == searchOrderId || x.DeliveryAddress.Contains(search));
            }
            else
            {
                query = query.Where(x => x.DeliveryAddress.Contains(search));
            }
        }

        query = sortBy?.ToLower() switch
        {
            "totalamount" => isAscending
                ? query.OrderBy(x => x.TotalAmount).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.TotalAmount).ThenByDescending(x => x.Id),
            "status" => isAscending
                ? query.OrderBy(x => x.Status).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.Status).ThenByDescending(x => x.Id),
            "createdatutc" or "createdat" => isAscending
                ? query.OrderBy(x => x.CreatedAtUtc).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id),
            _ => isAscending
                ? query.OrderBy(x => x.CreatedAtUtc).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id)
        };

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new OrderListItemDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                CustomerId = x.CustomerId,
                DeliveryAddress = x.DeliveryAddress,
                Status = x.Status,
                TotalAmount = x.TotalAmount,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc
            })
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<OrderListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNext = pageNumber < totalPages,
            HasPrevious = pageNumber > 1
        };
    }
    private async Task ChangeStatusAsync(
     Guid orderId,
     Guid tenantId,
     Guid userId,
     bool isTenantAdmin,
     OrderStatus nextStatus,
     string actionName,
     string? comment,
     int requestVersion,
     CancellationToken ct = default)
    {
        await EnsureTenantAccessAsync(tenantId, ct);

        if ((nextStatus == OrderStatus.Rejected || nextStatus == OrderStatus.Cancelled) &&
            string.IsNullOrWhiteSpace(comment))
        {
            throw new ArgumentException("A reason is required.", nameof(comment));
        }

        var query = _dbContext.Orders
            .Where(x => x.Id == orderId && x.TenantId == tenantId);

        if (!isTenantAdmin)
            query = query.Where(x => x.CustomerId == userId);

        var order = await query.FirstOrDefaultAsync(ct);

        if (order is null)
            throw new OrderNotFoundException(orderId, tenantId);

        if (order.Version != requestVersion)
            throw new InvalidOperationException("Order was modified. Please refresh and try again.");

        var currentStatus = order.Status;

        if (!OrderWorkflowRules.CanTransition(currentStatus.ToEnum<OrderStatus>(), nextStatus))
            throw new InvalidOrderTransitionException(orderId, currentStatus.ToEnum<OrderStatus>(), nextStatus);

        var now = DateTime.UtcNow;

        order.Status = nextStatus.ToString();
        order.UpdatedAtUtc = now;
        order.Version++;

        _dbContext.OrderStatusHistories.Add(new OrderStatusHistory
        {
            TenantId = tenantId,
            OrderId = order.Id,
            FromStatus = currentStatus,
            ToStatus = nextStatus.ToString(),
            ActionName = actionName,
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            ChangedBy = userId,
            ChangedAtUtc = now
        });

        try
        {
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException(
                "Order was modified by another request.",
                ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException(
                "Database error occurred while saving.",
                ex);
        }
    }

    private async Task EnsureTenantAccessAsync(Guid tenantId ,CancellationToken ct)
    {
        var hasAccess =await _tenantService.HasTenantAccessForCurrentUserAsync(tenantId,ct);
        if (!hasAccess)
            throw new UnauthorizedAccessException("User does not have access to this tenant.");
    }
}

