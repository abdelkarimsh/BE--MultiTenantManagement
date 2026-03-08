using AutoMapper.QueryableExtensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MultiTenantManagement.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiTenantManagement.Infrastructure.Features.Product.Dtos;
using MultiTenantManagement.Data.Models;
using MultiTenantManagement.Infrastructure.Helpers;
using MultiTenantManagement.Infrastructure.Features.Tenant;
using MultiTenantManagement.Infrastructure.Features.Users;

namespace MultiTenantManagement.Infrastructure.Features.Product
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ITenantService _tenantService;
        private readonly IUserManagementService _users;

        public ProductService(AppDbContext dbContext, IUserManagementService users, IMapper mapper, ITenantService tenantService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _tenantService = tenantService;
            _users = users;
        }

        public async Task<PagedResult<ProductDto>> GetPagedAsync(
            Guid tenantId,
            int pageNumber,
            int pageSize,
            string? sortBy,
            bool isAscending,
            string? search)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<MultiTenantManagement.Data.Models.Product> query = _dbContext.Products.AsNoTracking().Where(p=>tenantId==p.TenantId && !p.IsDeleted);

            // Filtering (search)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(p =>
                    (p.Name != null && p.Name.Contains(search)) ||
                    (p.Description != null && p.Description.Contains(search)));
            }

            // Sorting
            query = sortBy?.ToLower() switch
            {
                "name" => isAscending
                    ? query.OrderBy(p => p.Name)
                    : query.OrderByDescending(p => p.Name),

                "price" => isAscending
                    ? query.OrderBy(p => p.Price)
                    : query.OrderByDescending(p => p.Price),

                "createdatutc" or "createdat" => isAscending
                    ? query.OrderBy(p => p.CreatedAtUtc)
                    : query.OrderByDescending(p => p.CreatedAtUtc),

                _ => isAscending
                    ? query.OrderBy(p => p.Id)
                    : query.OrderByDescending(p => p.Id)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

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

        public async Task<ProductDto?> GetByIdAsync(Guid tenantId,Guid id)
        {
            var entity = await _dbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => tenantId == p.TenantId && p.Id == id );

            if (entity is null)
                return null;

            return _mapper.Map<ProductDto>(entity);
        }

        public async Task<ProductDto> CreateAsync(Guid tenantId, CreateProductDto dto)
        {

            var hasAccess = _tenantService.HasTenantAccessForCurrentUser(tenantId);
            if (!hasAccess)
                throw new UnauthorizedAccessException("User does not have access to this tenant.");
            dto.TenantId = tenantId;
            var entity = _mapper.Map<MultiTenantManagement.Data.Models.Product>(dto);
           ;
            await _dbContext.Products.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<ProductDto>(entity);
        }

        public async Task<bool> UpdateAsync(Guid tenantId,Guid id, UpdateProductDto dto)
        {

            var hasAccess = _tenantService.HasTenantAccessForCurrentUser(tenantId);
            if (!hasAccess) 
                throw new UnauthorizedAccessException("User does not have access to this tenant.");

            dto.TenantId = tenantId;
            dto.Id = id;
            var existing = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (existing is null)
                return false;

            _mapper.Map(dto, existing);

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid tenantId, Guid id)
        {
          
            var hasAccess = _tenantService.HasTenantAccessForCurrentUser(tenantId);
            if (!hasAccess)
                throw new UnauthorizedAccessException("User does not have access to this tenant.");


            var existing = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

                    if (existing is null)
                        return false;

            existing.IsDeleted = true;

            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}