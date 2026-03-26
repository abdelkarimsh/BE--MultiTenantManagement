using MultiTenantManagement.Core.Enums;

namespace MultiTenantManagement.Infrastructure.Features.Order.Exceptions;

public class InvalidOrderTransitionException : Exception
{
    public InvalidOrderTransitionException(Guid orderId, OrderStatus fromStatus, OrderStatus toStatus)
        : base($"Invalid order status transition for order '{orderId}': '{fromStatus}' -> '{toStatus}'.")
    {
    }
}
