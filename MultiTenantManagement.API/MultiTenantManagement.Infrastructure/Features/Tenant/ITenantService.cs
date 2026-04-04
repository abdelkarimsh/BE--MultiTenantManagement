using MultiTenantManagement.Infrastructure.Features.Tenant.Dtos;
using MultiTenantManagement.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Features.Tenant
{
    public interface ITenantService
    {
        Task<PagedResult<TenantDto>> GetPagedAsync(
                  int pageNumber,
                  int pageSize,
                  string? sortBy,
                  bool isAscending,
                  string? search,
                  bool? isActive,
                  CancellationToken ct);
        Task<TenantDto?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<TenantDto> CreateAsync(CreateTenantRequestDto req, CancellationToken ct);
        Task<bool> UpdateAsync(UpdateTenantRequestDto req, CancellationToken ct);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct);
        bool HasTenantAccessForCurrentUser(Guid routeTenantId);

        Task<TenantDto?> GetUserTenantAsync(CancellationToken ct);

    }
}
