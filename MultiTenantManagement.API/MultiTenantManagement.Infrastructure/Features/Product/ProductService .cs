using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using MultiTenantManagement.Data;
using MultiTenantManagement.Infrastructure.Features.Product.Dtos;
using MultiTenantManagement.Infrastructure.Features.Tenant;
using MultiTenantManagement.Infrastructure.Helpers;

namespace MultiTenantManagement.Infrastructure.Features.Product
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ITenantService _tenantService;

        public ProductService(
            AppDbContext dbContext,
            IMapper mapper,
            ITenantService tenantService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _tenantService = tenantService;
        }

        public async Task<PagedResult<ProductDto>> GetPagedAsync(
            Guid tenantId,
            int pageNumber,
            int pageSize,
            string? sortBy,
            bool isAscending,
            string? search,
            CancellationToken ct = default)
        {
            await EnsureTenantAccessAsync(tenantId, ct);

            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;
            pageSize = Math.Min(pageSize, 100);

            var query = _dbContext.Products
                .AsNoTracking()
                .Where(p => p.TenantId == tenantId && !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(p =>
                    (p.Name != null && p.Name.Contains(search)) ||
                    (p.Description != null && p.Description.Contains(search)));
            }

            query = sortBy?.ToLower() switch
            {
                "name" => isAscending
                    ? query.OrderBy(p => p.Name).ThenBy(p => p.Id)
                    : query.OrderByDescending(p => p.Name).ThenByDescending(p => p.Id),

                "price" => isAscending
                    ? query.OrderBy(p => p.Price).ThenBy(p => p.Id)
                    : query.OrderByDescending(p => p.Price).ThenByDescending(p => p.Id),

                "createdatutc" or "createdat" => isAscending
                    ? query.OrderBy(p => p.CreatedAtUtc).ThenBy(p => p.Id)
                    : query.OrderByDescending(p => p.CreatedAtUtc).ThenByDescending(p => p.Id),

                _ => isAscending
                    ? query.OrderBy(p => p.Id)
                    : query.OrderByDescending(p => p.Id)
            };

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResult<ProductDto>
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

        public async Task<ProductDto?> GetByIdAsync(
            Guid tenantId,
            Guid id,
            CancellationToken ct = default)
        {
            await EnsureTenantAccessAsync(tenantId, ct);

            var entity = await _dbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.TenantId == tenantId &&
                    p.Id == id &&
                    !p.IsDeleted,
                    ct);

            return entity is null ? null : _mapper.Map<ProductDto>(entity);
        }

        public async Task<ProductDto> CreateAsync(
            Guid tenantId,
            CreateProductDto dto,
            CancellationToken ct = default)
        {
            await EnsureTenantAccessAsync(tenantId, ct);

            dto.TenantId = tenantId;

            var entity = _mapper.Map<MultiTenantManagement.Data.Models.Product>(dto);
            await _dbContext.Products.AddAsync(entity, ct);
            await _dbContext.SaveChangesAsync(ct);

            return _mapper.Map<ProductDto>(entity);
        }

        public async Task<bool> UpdateAsync(
            Guid tenantId,
            Guid id,
            UpdateProductDto dto,
            CancellationToken ct = default)
        {
            await EnsureTenantAccessAsync(tenantId, ct);

            var existing = await _dbContext.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.TenantId == tenantId &&
                    !p.IsDeleted,
                    ct);

            if (existing is null)
                return false;

            if (existing.Version != dto.Version)
                throw new InvalidOperationException(
                    "Product was modified. Please refresh and try again.");

            dto.TenantId = tenantId;
            dto.Id = id;

            _mapper.Map(dto, existing);
            existing.Version++;

            try
            {
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException(
                    "Product was modified by another request.",
                    ex);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException(
                    "Database error occurred while saving.",
                    ex);
            }

            return true;
        }

        public async Task<bool> DeleteAsync(
            Guid tenantId,
            Guid id,
            int version,
            CancellationToken ct = default)
        {
            await EnsureTenantAccessAsync(tenantId, ct);

            var existing = await _dbContext.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.TenantId == tenantId &&
                    !p.IsDeleted,
                    ct);

            if (existing is null)
                return false;

            if (existing.Version != version)
                throw new InvalidOperationException(
                    "Product was modified. Please refresh and try again.");

            existing.IsDeleted = true;
            existing.Version++;

            try
            {
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException(
                    "Product was modified by another request.",
                    ex);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException(
                    "Database error occurred while saving.",
                    ex);
            }

            return true;
        }

        private async Task EnsureTenantAccessAsync(Guid tenantId, CancellationToken ct)
        {
            var hasAccess = await _tenantService.HasTenantAccessForCurrentUserAsync(tenantId, ct);

            if (!hasAccess)
                throw new UnauthorizedAccessException("User does not have access to this tenant.");
        }
    }
}