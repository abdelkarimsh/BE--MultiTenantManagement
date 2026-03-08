
using Microsoft.EntityFrameworkCore;
using MultiTenantManagement.Data;
using MultiTenantManagement.Infrastructure.Features.Tenant.Dtos;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MultiTenantManagement.Infrastructure.Helpers;

namespace MultiTenantManagement.Infrastructure.Features.Tenant
{
    public class TenantService : ITenantService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public TenantService(AppDbContext db, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;

        }

        public async Task<PagedResult<TenantDto>> GetPagedAsync(
                 int pageNumber,
                 int pageSize,
                 string? sortBy,
                 bool isAscending,
                 string? search,
                 bool? isActive,
                 CancellationToken ct)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<Data.Models.Tenant> query = _db.Tenants
                .AsNoTracking()
                .Where(t => !t.IsDeleted);  

            // Filtering
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(t =>
                    (t.Name != null && t.Name.Contains(search)));
            }

            // Sorting
            query = sortBy?.ToLower() switch
            {
                "name" => isAscending
                    ? query.OrderBy(t => t.Name)
                    : query.OrderByDescending(t => t.Name),

                "createdatutc" or "createdat" => isAscending
                    ? query.OrderBy(t => t.CreatedAtUtc)
                    : query.OrderByDescending(t => t.CreatedAtUtc),

                _ => isAscending
                    ? query.OrderBy(t => t.Id)
                    : query.OrderByDescending(t => t.Id)
            };

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<TenantDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResult<TenantDto>
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


        public async Task<TenantDto?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return await _db.Tenants
                .AsNoTracking()
                .Where(t => t.Id == id && !t.IsDeleted) 
                .Include(x => x.StoreSetting)
                .ProjectTo<TenantDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<TenantDto> CreateAsync(CreateTenantRequestDto req, CancellationToken ct)
        {
            var name = req.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tenant name is required.");

            if (await _db.Tenants.AnyAsync(t => t.SubDomain == req.SubDomain && !t.IsDeleted, ct))
                throw new ArgumentException("SubDomain must be unique.");

            var tenant = _mapper.Map<Data.Models.Tenant>(req);
   
            _db.Tenants.Add(tenant);
            await _db.SaveChangesAsync(ct);

            return _mapper.Map<TenantDto>(tenant);
        }

        public async Task<bool> UpdateAsync(UpdateTenantRequestDto req, CancellationToken ct)
        {
            var name = req.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tenant name is required.");

            var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == req.Id && !t.IsDeleted, ct);
            if (tenant is null) return false;

            if (await _db.Tenants.AnyAsync(t => t.SubDomain == req.SubDomain && t.Id != req.Id && !t.IsDeleted, ct))
                throw new ArgumentException("SubDomain must be unique.");

            tenant.Name = name;
            tenant.SubDomain = req.SubDomain;
            tenant.Status = req.Status.ToString();
            tenant.LogoUrl = req.LogoURL;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);
            if (tenant is null) return false;

            tenant.IsDeleted = true;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public bool HasTenantAccessForCurrentUser(Guid routeTenantId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return false;

            // SystemAdmin bypass
            bool isSystemAdmin =
                user.IsInRole("SystemAdmin") ||
                user.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "SystemAdmin") ||
                user.Claims.Any(c => c.Type == "role" && c.Value == "SystemAdmin");

            if (isSystemAdmin)
                return true;

            // TenantAdmin or normal user needs to match tenantId
            var claimTenantIdStr = user.Claims
                .FirstOrDefault(c => c.Type == "tenant_id")
                ?.Value;

            if (string.IsNullOrWhiteSpace(claimTenantIdStr))
                return false;

            // حاول تحويل claimTenantId إلى Guid وقارن
            if (!Guid.TryParse(claimTenantIdStr, out var claimTenantId))
                return false;

            // Compare route vs JWT
            return claimTenantId == routeTenantId;
        }


    }

}

