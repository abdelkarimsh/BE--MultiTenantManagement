using MultiTenantManagement.Infrastructure.Features.Product.Dtos;
using MultiTenantManagement.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Features.Product
{
    public interface IProductService
    {
        Task<PagedResult<ProductDto>> GetPagedAsync(Guid tenantId,
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool isAscending,
        string? search);

        Task<ProductDto?> GetByIdAsync(Guid tenantId, Guid id);
        Task<ProductDto> CreateAsync(Guid tenantId, CreateProductDto dto);
        Task<bool> UpdateAsync(Guid tenantId, Guid id, UpdateProductDto dto);
        Task<bool> DeleteAsync(Guid tenantId ,Guid id);
    }
}
