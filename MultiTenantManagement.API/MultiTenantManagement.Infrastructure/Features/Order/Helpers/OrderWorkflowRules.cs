using MultiTenantManagement.Core.Enums;

namespace MultiTenantManagement.Infrastructure.Features.Order.Helpers;

public static class OrderWorkflowRules
{
    private static readonly HashSet<(OrderStatus Current, OrderStatus Next)> AllowedTransitions = new()
    {
        (OrderStatus.PendingApproval, OrderStatus.Approved),
        (OrderStatus.PendingApproval, OrderStatus.Rejected),
        (OrderStatus.PendingApproval, OrderStatus.Cancelled),
        (OrderStatus.Approved, OrderStatus.Cancelled)
    };

    public static bool CanTransition(OrderStatus current, OrderStatus next)
        => AllowedTransitions.Contains((current, next));
}
