using MultiTenantManagement.Infrastructure.Features.Product.Dtos;
using MultiTenantManagement.Infrastructure.Helpers;


namespace MultiTenantManagement.Infrastructure.Features.Product
{
    public interface IProductService
    {
        Task<PagedResult<ProductDto>> GetPagedAsync(Guid tenantId,
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool isAscending,
        string? search,
        CancellationToken ct = default);

        Task<ProductDto?> GetByIdAsync(Guid tenantId, Guid id,
        CancellationToken ct = default);
        Task<ProductDto> CreateAsync(Guid tenantId, CreateProductDto dto,
        CancellationToken ct = default);
        Task<bool> UpdateAsync(Guid tenantId, Guid id, UpdateProductDto dto,
        CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid tenantId ,Guid id, int version,
        CancellationToken ct = default);
    }
}
