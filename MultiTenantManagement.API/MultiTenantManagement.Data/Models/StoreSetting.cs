namespace MultiTenantManagement.Data.Models;

public class StoreSetting
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public required string Currency { get; set; }
    public required string Theme { get; set; }
    public required string SupportPhone { get; set; }

    public Tenant? Tenant { get; init; }
}
