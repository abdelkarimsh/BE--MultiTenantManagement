namespace MultiTenantManagement.Infrastructure.Features.Order.Exceptions;

public class OrderNotFoundException : Exception
{
    public OrderNotFoundException(Guid orderId, Guid tenantId)
        : base($"Order '{orderId}' was not found for tenant '{tenantId}'.")
    {
    }
}
